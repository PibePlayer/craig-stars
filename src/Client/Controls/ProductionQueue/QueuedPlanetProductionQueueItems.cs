using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This component is used for 
    /// </summary>
    [Tool]
    public class QueuedPlanetProductionQueueItems : PlanetProductionQueueItems
    {
        static CSLog log = LogProvider.GetLogger(typeof(QueuedPlanetProductionQueueItems));

        internal readonly struct CompletionEstimate
        {
            public readonly int yearsToBuildOne;
            public readonly int yearsToBuildAll;
            public readonly float percentComplete;
            public CompletionEstimate(int yearsToBuildOne, int yearsToBuildAll, float percentComplete)
            {
                this.yearsToBuildOne = yearsToBuildOne;
                this.yearsToBuildAll = yearsToBuildAll;
                this.percentComplete = percentComplete;
            }
        }

        [Export]
        public bool ShowTopOfQueue { get; set; }

        public override void _Ready()
        {
            base._Ready();

            table.Data.AddColumn("Item");
            table.Data.AddColumn("Quantity", align: Label.AlignEnum.Right);
            table.ResetTable();
        }

        /// <summary>
        /// When a planet is updated, reset our internal Items list to whatever is currently in the planet queue
        /// </summary>
        protected override void OnPlanetUpdated()
        {
            base.OnPlanetUpdated();
            Items.Clear();
            if (Planet != null && Planet.ProductionQueue != null)
            {
                Planet.ProductionQueue.Items.ForEach(item => Items.Add(item.Clone()));
            }
        }

        protected override void OnSelectItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem metadata)
        {
            base.OnSelectItem(rowIndex, colIndex, cell, metadata);
            SelectedItemIndex = ShowTopOfQueue ? rowIndex - 1 : rowIndex;
            log.Debug($"Selected queue item index {SelectedItemIndex}");
        }

        public override void Clear()
        {
            base.Clear();
            AddTopOfQueueItem();
        }

        public override void UpdateItems()
        {
            if (Planet != null)
            {
                table.Data.ClearRows();

                AddQueudItems();

                table.SelectedRow = ShowTopOfQueue ? SelectedItemIndex + 1 : SelectedItemIndex;
                table.ResetRows();

                table.Update();

                // let any subscribers update based on the newly selected item
                PublishItemSelectedEvent(GetSelectedItem());
            }
        }

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
            // TODO: figure out skipped items
            var skipped = false;
            var color = GetColor(item.yearsToBuildOne, item.yearsToBuildAll, skipped);
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

            table.ResetRows();

            // re-select our selected item or select the top
            if (SelectedItemIndex >= 0 && SelectedItemIndex < Items.Count)
            {
                table.SelectedRow = ShowTopOfQueue ? SelectedItemIndex + 1 : SelectedItemIndex;
            }
            else
            {
                table.SelectedRow = 0;
            }

            table.Update();
        }

        void UpdateItemCompletionEstimates()
        {
            // go through each item and update it's YearsToComplete field
            Cost previousItemsCost = new Cost();
            foreach (var item in Items)
            {
                previousItemsCost = previousItemsCost - item.Allocated;
                // figure out how much this item costs
                var costOfOne = item.GetCostOfOne(RulesManager.Rules, Me);
                if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    costOfOne = Planet.Starbase.GetUpgradeCost(item.Design);
                }

                var estimate = GetCompletionEstimate(costOfOne, previousItemsCost, item);
                item.yearsToBuildAll = estimate.yearsToBuildAll;
                item.yearsToBuildOne = estimate.yearsToBuildOne;
                item.percentComplete = estimate.percentComplete;

                // increase our previousItemsCost for the next item
                previousItemsCost += costOfOne * item.Quantity;
            }
        }

        void AddTopOfQueueItem()
        {
            if (ShowTopOfQueue)
            {
                table.Data.AddRow(Planet.ProductionQueue.Items.Count == 0 ? "-- Queue is Empty --" : "-- Top of the Queue --", "");
            }
        }

        /// <summary>
        /// Get the estimated number of years to complete this item, based on it's location in the queue
        /// and the costs of all items before it
        /// </summary>
        /// <param name="costOfOne">The cost of this item</param>
        CompletionEstimate GetCompletionEstimate(Cost costOfOne, Cost previousItemsCost, ProductionQueueItem item)
        {
            // Get the total cost of this item plus any previous items in the queue
            // and subtract what we have on hand (that will be applied this year)
            Cost remainingCostOfOne = costOfOne + previousItemsCost - availableCost;
            Cost costOfAll = (costOfOne * item.Quantity);
            Cost remainingCostOfAll = costOfAll + previousItemsCost - availableCost;

            // If we have a bunch of leftover minerals because our planet is full, 0 those out
            remainingCostOfOne = new Cost(
                Math.Max(0, remainingCostOfOne.Ironium),
                Math.Max(0, remainingCostOfOne.Boranium),
                Math.Max(0, remainingCostOfOne.Germanium),
                Math.Max(0, remainingCostOfOne.Resources)
            );
            remainingCostOfAll = new Cost(
                Math.Max(0, remainingCostOfAll.Ironium),
                Math.Max(0, remainingCostOfAll.Boranium),
                Math.Max(0, remainingCostOfAll.Germanium),
                Math.Max(0, remainingCostOfAll.Resources)
            );

            float percentComplete = item.Allocated != Cost.Zero ? Mathf.Clamp(item.Allocated / costOfAll, 0, 1) : 0;
            int yearsToBuildAll = remainingCostOfAll == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCostOfAll / yearlyAvailableCost, 1, int.MaxValue));
            int yearsToBuildOne = remainingCostOfOne == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCostOfOne / yearlyAvailableCost, 1, int.MaxValue));
            return new CompletionEstimate(yearsToBuildOne, yearsToBuildAll, percentComplete);
        }

        /// <summary>
        /// Add an item to the queue with some quantity
        /// The item is either added to the top of the queue, added to the end, inserted, or added to the current
        /// item if they are the same type
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        public void AddItem(ProductionQueueItem item, int quantity)
        {
            var selectedQueueItem = GetSelectedItem();
            if (selectedQueueItem != null
                && selectedQueueItem.Type == item.Type &&
                selectedQueueItem.Design == item.Design)
            {
                var newItem = selectedQueueItem;
                selectedQueueItem.Quantity += quantity;
            }
            else
            {
                item.Quantity = quantity;
                if (selectedQueueItem != null)
                {
                    // add below the selected item
                    if (Items.Count > SelectedItemIndex + 1)
                    {
                        var nextItem = Items[SelectedItemIndex + 1];
                        if (nextItem.Type == item.Type && nextItem.Design == item.Design)
                        {
                            // increase quantity
                            nextItem.Quantity += quantity;
                            SelectedItemIndex = SelectedItemIndex + 1;
                        }
                        else
                        {
                            Items.Insert(SelectedItemIndex + 1, item);
                            log.Debug($"Inserted new item at {SelectedItemIndex + 1} - {item}");
                            SelectedItemIndex = SelectedItemIndex + 1;
                        }
                    }
                    else
                    {
                        Items.Insert(SelectedItemIndex + 1, item);
                        log.Debug($"Inserted new item at {SelectedItemIndex + 1} - {item}");
                        SelectedItemIndex = SelectedItemIndex + 1;
                    }
                }
                else
                {
                    Items.Insert(0, item);
                    SelectedItemIndex = 0;
                    log.Debug($"Added new item to at {SelectedItemIndex} - {item}");
                }
            }
            UpdateItems();

        }

        public void RemoveItem(int quantity)
        {
            var selectedQueueItem = GetSelectedItem();
            if (selectedQueueItem != null)
            {
                selectedQueueItem.Quantity -= quantity;
                if (selectedQueueItem.Quantity <= 0)
                {
                    Items.RemoveAt(SelectedItemIndex);
                    if (SelectedItemIndex > Items.Count && Items.Count > 0)
                    {
                        // if we removed the last item, set our index to the next last item
                        SelectedItemIndex = Items.Count;
                    }
                    else
                    {
                        SelectedItemIndex = SelectedItemIndex - 1;
                    }
                }
                UpdateItems();
            }
        }

        public void ItemUp()
        {
            if (SelectedItemIndex > 0 && SelectedItemIndex < Items.Count)
            {
                // swap items and redraw the tree
                var selectedItem = Items[SelectedItemIndex];
                var previousItem = Items[SelectedItemIndex - 1];
                Items[SelectedItemIndex] = previousItem;
                Items[SelectedItemIndex - 1] = selectedItem;
                SelectedItemIndex--;
                UpdateItems();
            }
        }

        public void ItemDown()
        {
            if (SelectedItemIndex >= 0 && SelectedItemIndex < Items.Count - 1)
            {
                // swap items and redraw the tree
                var selectedItem = Items[SelectedItemIndex];
                var nextItem = Items[SelectedItemIndex + 1];
                Items[SelectedItemIndex] = nextItem;
                Items[SelectedItemIndex + 1] = selectedItem;
                SelectedItemIndex++;
                UpdateItems();
            }

        }
    }
}