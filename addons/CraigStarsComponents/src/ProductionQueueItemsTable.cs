using CraigStarsTable;
using Godot;
using System;

namespace CraigStars
{
    public class ProductionQueueItemsTable : Table<ProductionQueueItem>
    {
        public override void _Ready()
        {
            base._Ready();
            CellControlScene = "res://addons/CraigStarsComponents/src/ProductionQueue/ProductionQueueItemLabelCell.tscn";
            ShowHeader = false;
            BorderStyle = BorderStyle.None;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            SizeFlagsVertical = (int)SizeFlags.ExpandFill;

            ResetTable();
        }

    }
}