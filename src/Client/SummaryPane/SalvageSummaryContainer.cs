using Godot;
using System;
using CraigStars.Singletons;
using log4net;
using CraigStars.Utils;

namespace CraigStars
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