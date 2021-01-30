using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class FleetWaypointTile : FleetTile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetWaypointTaskTile));

        /// <summary>
        /// The index of the currently selected waypoint, or -1 if none selected
        /// </summary>
        public int ActiveWaypointIndex { get; set; } = -1;

        /// <summary>
        /// Return a copy of the active waypoint, or null if we don't have one
        /// </summary>
        public Waypoint? ActiveWaypoint
        {
            get
            {
                if (ActiveFleet != null && ActiveWaypointIndex >= 0 && ActiveWaypointIndex < ActiveFleet.Fleet.Waypoints.Count)
                {
                    return ActiveFleet.Fleet.Waypoints[ActiveWaypointIndex];
                }
                return null;
            }
        }

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

        /// <summary>
        /// Update the ActiveWaypoint with a new value
        /// </summary>
        /// <param name="waypoint"></param>
        public void UpdateActiveWaypoint(Waypoint waypoint)
        {
            if (ActiveFleet != null && ActiveWaypointIndex >= 0 && ActiveWaypointIndex < ActiveFleet.Fleet.Waypoints.Count)
            {
                ActiveFleet.Fleet.Waypoints[ActiveWaypointIndex] = waypoint;
            }
            else
            {
                log.Error($"Tried to update the active waypoint, but we don't have one at {ActiveWaypointIndex}");
            }
        }

        void OnWaypointSelected(Waypoint waypoint, int index)
        {
            log.Debug($"Selected waypoint {waypoint.TargetName} index: {index}");
            ActiveWaypointIndex = index;
            UpdateControls();
        }

        void OnWaypointAdded(Fleet fleet, Waypoint waypoint, int index)
        {
            log.Debug($"Added waypoint {waypoint.TargetName} index: {index}");
            ActiveWaypointIndex = index;
            UpdateControls();
        }

        void OnWaypointDeleted(Waypoint waypoint, int index)
        {
            log.Debug($"Deleted waypoint {waypoint.TargetName} index: {index}");
            if (ActiveWaypointIndex == index)
            {
                ActiveWaypointIndex = -1;
            }
            UpdateControls();
        }

    }
}