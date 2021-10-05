using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigStars.Client
{
    /// <summary>
    /// Show items that belong to a ProductionPlan
    /// </summary>
    public class ProductionPlanItems : ProductionQueueItems
    {
        static CSLog log = LogProvider.GetLogger(typeof(ProductionPlanItems));

        public override void _Ready()
        {
            base._Ready();

            table.Data.AddColumn("Item");
            table.Data.AddColumn("Quantity", align: Label.AlignEnum.Right);
            table.ResetTable();
        }

        public async override Task UpdateItems()
        {
            table.Data.ClearRows();
            Items.ForEach(item => AddItem(item));
            await table.ResetRows();
        }

        void AddItem(ProductionQueueItem item)
        {
            Color color = Colors.White;
            bool italic = item.IsAuto;

            table.Data.AddRowAdvanced(metadata: item, color: color, italic: italic, item.FullName, item.Quantity);
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
                            Items.Insert(SelectedItemIndex, item);
                            log.Debug($"Inserted new item at {SelectedItemIndex + 1} - {item}");
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
                    log.Debug($"Added new item at {SelectedItemIndex} - {item}");
                }
            }
            var _ = UpdateItems();
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
                var _ = UpdateItems();
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
                var _ = UpdateItems();
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
                var _ = UpdateItems();
            }

        }
    }
}