using CraigStarsTable;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class ProductionQueueItemsTable : Table<ProductionQueueItem>
    {
        public ProductionQueueItemsTable() : base()
        {
            CellControlScene = "res://addons/CraigStarsComponents/src/ProductionQueue/ProductionQueueItemLabelCell.tscn";
            ShowHeader = false;
            BorderStyle = BorderStyle.None;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        }

    }
}