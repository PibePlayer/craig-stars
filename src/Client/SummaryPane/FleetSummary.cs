using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class FleetSummary : Control
    {
        protected Player Me { get => PlayersManager.Me; }

        protected FleetSprite Fleet
        {
            get => fleet;
            set
            {
                fleet = value;
                UpdateControls();
            }
        }
        FleetSprite fleet;

        public override void _Ready()
        {
            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            Fleet = mapObject as FleetSprite;
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            Fleet = null;
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}