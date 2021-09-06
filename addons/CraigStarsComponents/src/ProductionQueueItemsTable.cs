using CraigStarsTable;
using Godot;
using System;

namespace CraigStars.Client
{
    [Tool]
    public class ProductionQueueItemsTable : Table<ProductionQueueItem>
    {
        public ProductionQueueItemsTable() : base()
        {
            ShowHeader = false;
            BorderStyle = BorderStyle.None;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        }

    }
}