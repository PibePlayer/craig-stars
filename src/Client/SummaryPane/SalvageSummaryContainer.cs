using System;
using Godot;

namespace CraigStars.Client
{
    public class SalvageSummaryContainer : MapObjectSummary<SalvageSprite>
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldSummaryContainer));

        CargoGrid cargoGrid;

        public override void _Ready()
        {
            base._Ready();
            cargoGrid = GetNode<CargoGrid>("HBoxContainer/CargoGrid");
        }

        protected override void UpdateControls()
        {
            if (MapObject != null)
            {
                cargoGrid.Cargo = MapObject.Salvage.Cargo;
            }
        }
    }
}