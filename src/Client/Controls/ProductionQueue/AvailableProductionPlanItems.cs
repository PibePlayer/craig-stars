using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CraigStars.Client
{
    /// <summary>
    /// Show items that can be added to a production plan
    /// </summary>
    public class AvailableProductionPlanItems : ProductionQueueItems
    {
        public override void _Ready()
        {
            base._Ready();

            table.Data.AddColumn("Item", sortable: false);
            table.ResetTable();
        }

        public async override Task UpdateItems()
        {
            table.Data.ClearRows();
            AddAvailableItems();
            await table.ResetRows();
        }

        /// <summary>
        /// Add any items to the table that this planet can build
        /// </summary>
        void AddAvailableItems()
        {
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactories));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMines));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefenses));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralAlchemy));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMaxTerraform));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMinTerraform));
        }

        void AddAvailableItem(ProductionQueueItem item)
        {
            Items.Add(item);
            Color color = Colors.White;
            bool italic = item.IsAuto;

            table.Data.AddRowAdvanced(metadata: item, color: color, italic: italic, item.FullName);
        }
    }
}