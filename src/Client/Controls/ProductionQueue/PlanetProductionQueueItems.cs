using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This component is used for 
    /// </summary>
    public abstract class PlanetProductionQueueItems : ScrollContainer
    {
        public const int NoItemSelected = -1;
        public delegate void RowAction(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item);
        public event RowAction RowSelectedEvent;
        public event RowAction RowActivatedEvent;
        protected void PublishRowSelectedEvent(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item) => RowSelectedEvent?.Invoke(rowIndex, colIndex, cell, item);
        protected void PublishRowActivatedEvent(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item) => RowActivatedEvent?.Invoke(rowIndex, colIndex, cell, item);

        protected Player Me { get => PlayersManager.Me; }

        public Planet Planet
        {
            get => planet;
            set
            {
                planet = value;
                // update the items when the planet changes
                OnPlanetUpdated();
                UpdateItems();
            }
        }
        Planet planet;

        public bool ContributesOnlyLeftoverToResearch { get; set; }

        public List<ProductionQueueItem> Items { get; set; } = new List<ProductionQueueItem>();
        public int SelectedItemIndex = NoItemSelected;

        protected ProductionQueueItemTable table;

        protected Cost availableCost = Cost.Zero;
        protected Cost yearlyAvailableCost = Cost.Zero;

        public override void _Ready()
        {
            base._Ready();

            table = GetNode<ProductionQueueItemTable>("Table");

            table.RowSelectedEvent += OnSelectItem;
            table.RowActivatedEvent += OnActivateItem;

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            table.RowSelectedEvent -= OnSelectItem;
            table.RowActivatedEvent -= OnActivateItem;
        }

        /// <summary>
        /// When a planet is updated, reset our internal Items list to whatever is currently in the planet queue
        /// </summary>
        protected virtual void OnPlanetUpdated()
        {
            Items.Clear();
            if (Planet != null)
            {
                ComputeAvailableResources();
            }
        }

        protected virtual void OnActivateItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem metadata)
        {
            RowActivatedEvent?.Invoke(rowIndex, colIndex, cell, metadata);
        }

        protected virtual void OnSelectItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem metadata)
        {
            SelectedItemIndex = rowIndex;
            if (metadata != null)
            {
                RowSelectedEvent?.Invoke(rowIndex, colIndex, cell, metadata);
            }
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            if (Visible)
            {
                UpdateItems();
                if (table.Data.SourceRows.Count > 0)
                {
                    table.SelectedRow = 0;
                }
            }
        }

        public virtual void Clear()
        {
            table.Data.ClearRows();
        }

        public abstract void UpdateItems();

        /// <summary>
        /// Get the currently selected item, or null if the top of the queue is selected
        /// </summary>
        /// <returns></returns>
        public ProductionQueueItem GetSelectedItem()
        {
            if (SelectedItemIndex >= 0 && SelectedItemIndex < Items.Count)
            {
                return Items[SelectedItemIndex];
            }
            return null;
        }

        /// <summary>
        /// Based on the planet, compute how many resources we have available
        /// as well as minerals for building.
        /// </summary>
        protected void ComputeAvailableResources()
        {
            if (Planet != null)
            {
                // figure out how many resources we have per year
                int yearlyResources = 0;
                if (ContributesOnlyLeftoverToResearch)
                {
                    yearlyResources = Planet.ResourcesPerYear;
                }
                else
                {
                    yearlyResources = Planet.ResourcesPerYearAvailable;
                }

                // this is how man resources and minerals our planet produces each year
                yearlyAvailableCost = new Cost(Planet.MineralOutput, yearlyResources);

                // Get the total availble cost of this planet's yearly output + resources on hand.
                availableCost = yearlyAvailableCost + Planet.Cargo.ToCost();
            }
            else
            {
                yearlyAvailableCost = Cost.Zero;
                availableCost = Cost.Zero;
            }
        }

        #region Available Items

        /// <summary>
        /// Add any items to the table that this planet can build
        /// </summary>
        void AddAvailableItems()
        {
            // add each design
            if (Planet.HasStarbase && Planet.Starbase.DockCapacity > 0)
            {
                Me.Designs.ForEach(design =>
                {
                    if (Planet.CanBuild(Me, design.Aggregate.Mass))
                    {
                        if (design.Hull.Starbase)
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.Starbase, design: design));
                        }
                        else
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, design: design));
                        }
                    }
                });

                if (Planet.HasMassDriver)
                {
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.IroniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.BoraniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.GermaniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.MixedMineralPacket));
                }
            }

            // add each type of item.
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Factory));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Mine));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Defenses));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.MineralAlchemy));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactories));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMines));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefenses));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralAlchemy));
            if (Planet.HasMassDriver)
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralPacket));

            }

            void AddAvailableItem(ProductionQueueItem item)
            {
                Items.Add(item);
                Color color = Colors.White;
                bool italic = false;
                if (item.IsAuto)
                {
                    // make italic
                    italic = true;
                }
                else
                {

                    var cost = item.GetCostOfOne(RulesManager.Rules, Me);

                    // Get the total cost of this item plus any previous items in the queue
                    // and subtract what we have on hand (that will be applied this year)
                    Cost remainingCost = cost - availableCost;

                    // If we have a bunch of leftover minerals because our planet is full, 0 those out
                    remainingCost = new Cost(
                        Math.Max(0, remainingCost.Ironium),
                        Math.Max(0, remainingCost.Boranium),
                        Math.Max(0, remainingCost.Germanium),
                        Math.Max(0, remainingCost.Resources)
                    );

                    int numYearsToBuild = remainingCost == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCost / yearlyAvailableCost, 1, int.MaxValue));

                    color = numYearsToBuild > 1 ? Colors.Blue : Colors.Green;
                }

                table.Data.AddRowAdvanced(metadata: item, color: color, italic: italic, item.FullName);
            }
        }

        #endregion

        #region Queued Items

        void AddQueudItems()
        {
            if (Planet.ProductionQueue != null)
            {
                AddTopOfQueueItem();
                UpdateItemCompletionEstimates();

                Items.Each((item, index) =>
                {
                    AddQueuedItem(item, index);
                });
            }

        }

        void AddQueuedItem(ProductionQueueItem item, int index)
        {
            var italic = item.IsAuto;
            var color = Colors.White;
            table.Data.AddRowAdvanced(metadata: item, color: color, italic: italic, item.FullName, item.Quantity);
        }

        void UpdateQueuedItems()
        {
            table.Data.ClearRows();
            AddTopOfQueueItem();

            UpdateItemCompletionEstimates();

            // add the item to the tree
            Items.Each((item, index) =>
            {
                AddQueuedItem(item, index++);
            });

            // re-select our selected item or select the top
            if (SelectedItemIndex >= 0 && SelectedItemIndex < Items.Count)
            {
                table.SelectedRow = SelectedItemIndex + 1;
            }
            else
            {
                table.SelectedRow = 0;
            }

            table.ResetTable();
        }

        void UpdateItemCompletionEstimates()
        {
            var allocatedSoFar = Planet.ProductionQueue.Allocated;
            Cost previousItemsCost = -allocatedSoFar;

            // go through each item and update it's YearsToComplete field
            int index = 0;
            Items = Items.Select(item =>
            {
                // figure out how much this item costs
                var cost = item.GetCostOfOne(RulesManager.Rules, Me) * item.Quantity;
                if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    cost = Planet.Starbase.GetUpgradeCost(item.Design);
                }

                var (yearsToBuild, percentComplete) = GetCompletionEstimate(cost, previousItemsCost, index++);
                item.yearsToBuild = yearsToBuild;
                item.percentComplete = percentComplete;

                // increase our previousItemsCost for the next item
                previousItemsCost += cost;

                return item;
            }).ToList();
        }

        void AddTopOfQueueItem()
        {
            table.Data.AddRow(Planet.ProductionQueue.Items.Count == 0 ? "-- Queue is Empty --" : "-- Top of the Queue --", "");
        }

        /// <summary>
        /// Get the estimated number of years to complete this item, based on it's location in the queue
        /// and the costs of all items before it
        /// </summary>
        /// <param name="cost">The cost of this item</param>
        (int, float) GetCompletionEstimate(Cost cost, Cost previousItemsCost, int index)
        {
            // see how much has been allocated for the top item in the queue
            var allocatedSoFar = Planet.ProductionQueue.Allocated;

            // Get the total cost of this item plus any previous items in the queue
            // and subtract what we have on hand (that will be applied this year)
            Cost remainingCost = cost + previousItemsCost - availableCost;

            // If we have a bunch of leftover minerals because our planet is full, 0 those out
            remainingCost = new Cost(
                Math.Max(0, remainingCost.Ironium),
                Math.Max(0, remainingCost.Boranium),
                Math.Max(0, remainingCost.Germanium),
                Math.Max(0, remainingCost.Resources)
            );

            float percentComplete = index == 0 && allocatedSoFar != Cost.Zero ? Mathf.Clamp(allocatedSoFar / cost, 0, 1) : 0;
            int yearsToBuild = remainingCost == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCost / yearlyAvailableCost, 1, int.MaxValue));
            return (yearsToBuild, percentComplete);
        }

        #endregion

    }
}