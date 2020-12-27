using Godot;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;

namespace CraigStars
{
    public class Viewport : Node2D
    {
        PackedScene waypointAreaScene;

        public Universe Universe { get; private set; }
        public List<WaypointArea> waypointAreas = new List<WaypointArea>();

        public Fleet ActiveFleet
        {
            get => activeFleet; set
            {
                if (activeFleet != value)
                {
                    activeFleet = value;
                    OnActiveFleetChanged();
                }
            }
        }
        Fleet activeFleet;

        public Planet ActivePlanet { get; set; }

        public override void _Ready()
        {
            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/WaypointArea.tscn");
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent += OnMapObjectWaypointAdded;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent -= OnMapObjectWaypointAdded;
        }

        void OnMapObjectActivated(MapObject mapObject)
        {
            ActiveFleet = mapObject as Fleet;
            ActivePlanet = mapObject as Planet;
        }

        void OnMapObjectWaypointAdded(MapObject mapObject)
        {
            if (ActiveFleet != null)
            {
                var waypoint = ActiveFleet.AddWaypoint(mapObject);
                AddWaypointArea(waypoint);
            }
        }

        /// <summary>
        /// When the ActiveFleet changes, 
        /// </summary>
        void OnActiveFleetChanged()
        {
            waypointAreas.ForEach(wpa => { wpa.QueueFree(); });
            waypointAreas.Clear();
            ActiveFleet?.Waypoints.ForEach(wp => AddWaypointArea(wp));
        }

        void AddWaypointArea(Waypoint waypoint)
        {
            var waypointArea = waypointAreaScene.Instance() as WaypointArea;
            waypointArea.Waypoint = waypoint;
            waypointAreas.Add(waypointArea);
            AddChild(waypointArea);
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
