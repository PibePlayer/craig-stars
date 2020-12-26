using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class Viewport : Node2D
    {
        public Universe Universe { get; private set; }

        public Fleet ActiveFleet { get; set; }
        public Planet ActivePlanet { get; set; }

        public override void _Ready()
        {
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent += OnMapObjectWaypointAdded;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        }

        void OnMapObjectActivated(MapObject mapObject)
        {
            ActiveFleet = mapObject as Fleet;
            ActivePlanet = mapObject as Planet;
        }

        void OnMapObjectWaypointAdded(MapObject mapObject)
        {
            if (ActiveFleet != null) {
                ActiveFleet.AddWaypoint(mapObject);
            }
        }

        public void AddUniverse(Universe universe)
        {
            AddChild(universe);
            Universe = universe;
            CallDeferred(nameof(UpdateViewport));
        }

        public void UpdateViewport()
        {
            Universe.Planets.ForEach(p => p.UpdateVisibleSprites());
        }

    }
}
