using System;
using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class FleetWaypointTile : FleetTile
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetWaypointTile));

        /// <summary>
        /// Return a copy of the active waypoint, or null if we don't have one
        /// </summary>
        public Waypoint ActiveWaypoint { get; set; }

        public override void _Ready()
        {
            base._Ready();
            EventManager.WaypointSelectedEvent += OnWaypointSelected;
            EventManager.WaypointAddedEvent += OnWaypointAdded;
            EventManager.WaypointDeletedEvent += OnWaypointDeleted;
            EventManager.WaypointMovedEvent += OnWaypointMoved;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            EventManager.WaypointSelectedEvent -= OnWaypointSelected;
            EventManager.WaypointAddedEvent -= OnWaypointAdded;
            EventManager.WaypointDeletedEvent -= OnWaypointDeleted;
            EventManager.WaypointMovedEvent -= OnWaypointMoved;
        }

        void OnWaypointSelected(Waypoint waypoint)
        {
            log.Debug($"Selected waypoint {waypoint.TargetName}");
            ActiveWaypoint = waypoint;
            UpdateControls();
        }

        void OnWaypointAdded(Fleet fleet, Waypoint waypoint)
        {
            log.Debug($"Added waypoint {waypoint.TargetName}");
            ActiveWaypoint = waypoint;
            UpdateControls();
        }

        void OnWaypointDeleted(Waypoint waypoint)
        {
            log.Debug($"Deleted waypoint {waypoint.TargetName}");
            if (ActiveWaypoint == waypoint)
            {
                ActiveWaypoint = null;
            }
            UpdateControls();
        }

        void OnWaypointMoved(Fleet fleet, Waypoint waypoint)
        {
            log.Debug($"Moved waypoint {waypoint.TargetName}");
            ActiveWaypoint = waypoint;
            UpdateControls();
        }


    }
}