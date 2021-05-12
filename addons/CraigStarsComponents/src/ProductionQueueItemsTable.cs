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
            CellControlScript = "res://addons/CraigStarsComponents/src/ProductionQueueItemLabelCell.cs";
            ShowHeader = false;
            BorderStyle = BorderStyle.None;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        }

    }
}