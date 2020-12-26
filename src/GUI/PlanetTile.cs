using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetTile : MarginContainer
    {
        public Planet ActivePlanet { get; set; }

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

        protected virtual void OnTurnPassed(int year)
        {
            UpdateControls();
        }

        protected virtual void OnMapObjectActivated(MapObject mapObject)
        {
            ActivePlanet = mapObject as Planet;
            Visible = ActivePlanet != null;
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}
