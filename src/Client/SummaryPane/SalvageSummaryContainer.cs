using Godot;
using System;
using CraigStars.Singletons;
using log4net;
using CraigStars.Utils;

namespace CraigStars
{
    public class SalvageSummaryContainer : Container
    {
        static ILog log = LogManager.GetLogger(typeof(MineFieldSummaryContainer));

        SalvageSprite salvage;
        CargoGrid cargoGrid;

        public override void _Ready()
        {
            base._Ready();
            cargoGrid = GetNode<CargoGrid>("HBoxContainer/CargoGrid");

            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            salvage = mapObject as SalvageSprite;
            UpdateControls();
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            salvage = null;
            UpdateControls();
        }

        void UpdateControls()
        {
            if (salvage != null)
            {
                cargoGrid.Cargo = salvage.Salvage.Cargo;
            }
        }
    }
}