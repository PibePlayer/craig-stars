using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// This component is used for a table of production queue items
    /// It has various events that trigger when items are selected or activated (double clicked)
    /// </summary>
    public abstract class ProductionQueueItems : ScrollContainer
    {
        public const int NoItemSelected = -1;
        public delegate void ItemAction(ProductionQueueItem item);
        public event ItemAction ItemSelectedEvent;
        public event ItemAction ItemActivatedEvent;
        protected void PublishItemSelectedEvent(ProductionQueueItem item) => ItemSelectedEvent?.Invoke(item);
        protected void PublishItemActivatedEvent(ProductionQueueItem item) => ItemActivatedEvent?.Invoke(item);

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        public List<ProductionQueueItem> Items { get; set; } = new List<ProductionQueueItem>();
        public int SelectedItemIndex = NoItemSelected;

        protected ProductionQueueItemsTable table;

        public override void _Ready()
        {
            base._Ready();

            table = GetNode<ProductionQueueItemsTable>("ProductionQueueItemsTable");

            table.RowSelectedEvent += OnSelectItem;
            table.RowActivatedEvent += OnActivateItem;

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                if (table != null)
                {
                    table.RowSelectedEvent -= OnSelectItem;
                    table.RowActivatedEvent -= OnActivateItem;
                }
            }
        }

        protected virtual void OnActivateItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem metadata)
        {
            ItemActivatedEvent?.Invoke(metadata);
        }

        protected virtual void OnSelectItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem metadata)
        {
            SelectedItemIndex = rowIndex;
            if (metadata != null)
            {
                ItemSelectedEvent?.Invoke(metadata);
            }
        }

        /// <summary>
        /// Asynchronous function to update items. If the items never change (i.e. the available items)
        /// this returns complete
        /// </summary>
        /// <returns></returns>
        public virtual Task UpdateItems() => Task.CompletedTask;

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected async virtual void OnVisibilityChanged()
        {
            if (IsVisibleInTree() && table != null)
            {
                await UpdateItems();
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

    }
}