using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class FleetWaypointTile : FleetTile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetWaypointTaskTile));

        /// <summary>
        /// Return a copy of the active waypoint, or null if we don't have one
        /// </summary>
        public Waypoint ActiveWaypoint { get; set; }

        public override void _Ready()
        {
            base._Ready();
            Signals.WaypointSelectedEvent += OnWaypointSelected;
            Signals.WaypointAddedEvent += OnWaypointAdded;
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.WaypointSelectedEvent -= OnWaypointSelected;
            Signals.WaypointAddedEvent -= OnWaypointAdded;
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
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

    }
}