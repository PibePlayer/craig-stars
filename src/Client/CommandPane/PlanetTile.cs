using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetTile : MarginContainer
    {
        public PlanetSprite ActivePlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        public override void _Ready()
        {
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        protected virtual void OnTurnPassed(PublicGameInfo gameInfo)
        {
            ActivePlanet = null;
            UpdateControls();
        }

        protected virtual void OnMapObjectActivated(MapObjectSprite mapObject)
        {
            ActivePlanet = mapObject as PlanetSprite;
            Visible = ActivePlanet != null;
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}
