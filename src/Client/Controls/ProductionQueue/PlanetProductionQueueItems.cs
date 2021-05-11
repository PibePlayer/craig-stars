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
    public abstract class PlanetProductionQueueItems : ScrollContainer
    {
        public const int NoItemSelected = -1;
        public delegate void RowAction(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item);
        public event RowAction RowSelectedEvent;
        public event RowAction RowActivatedEvent;
        protected void PublishRowSelectedEvent(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item) => RowSelectedEvent?.Invoke(rowIndex, colIndex, cell, item);
        protected void PublishRowActivatedEvent(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item) => RowActivatedEvent?.Invoke(rowIndex, colIndex, cell, item);

        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

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

        protected ProductionQueueItemsTable table;

        protected Cost availableCost = Cost.Zero;
        protected Cost yearlyAvailableCost = Cost.Zero;

        public override void _Ready()
        {
            base._Ready();

            table = GetNode<ProductionQueueItemsTable>("ProductionQueueItemsTable");

            table.RowSelectedEvent += OnSelectItem;
            table.RowActivatedEvent += OnActivateItem;

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (table != null)
            {
                table.RowSelectedEvent -= OnSelectItem;
                table.RowActivatedEvent -= OnActivateItem;
            }
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
            if (Visible && table != null)
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
        /// Get the color for this ProductionQueueItem based on how many years it takes to build
        /// </summary>
        /// <param name="yearsToBuild"></param>
        /// <param name="skipped">true if this item will be skipped (i.e. an auto item that is already at max usable items)</param>
        /// <returns></returns>
        public Color GetColor(int yearsToBuild, bool skipped)
        {
            if (skipped)
            {
                return GUIColors.ProductionQueueSkippedColor;
            }
            if (yearsToBuild <= 1)
            {
                return GUIColors.ProductionQueueItemOneYearColor;
            }
            else if (yearsToBuild > 1)
            {
                return GUIColors.ProductionQueueMoreThanOneYearColor;
            }
            else if (yearsToBuild >= 100)
            {
                return GUIColors.ProductionQueueNeverBuildColor;
            }

            return Colors.White;
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


    }
}