using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// During waypoint processing, we process waypoints in groups, i.e.
    /// all unloads, then all colonizations, etc. The FleetWaypoint
    /// is used in processing
    /// </summary>
    internal class FleetWaypoint
    {
        public Fleet Fleet { get; set; }
        public Waypoint Waypoint { get; set; }
        public Player Player { get; set; }

        /// <summary>
        /// For transport tasks, this contains all the minerals that are loaded or unloaded, depending
        /// on what category this FleetWaypoint is in
        /// </summary>
        /// <typeparam name="WaypointTransportTask"></typeparam>
        /// <returns></returns>
        public List<FleetWaypointTransportTask> Tasks { get; set; } = new List<FleetWaypointTransportTask>();

        public FleetWaypoint(Fleet fleet, Waypoint waypoint, Player player)
        {
            Fleet = fleet;
            Waypoint = waypoint;
            Player = player;
        }

        public FleetWaypoint(Fleet fleet, Waypoint waypoint, List<FleetWaypointTransportTask> tasks)
        {
            Fleet = fleet;
            Waypoint = waypoint;
            Tasks = tasks;
        }

        public void AddTask(WaypointTransportTask task, CargoType cargoType)
        {
            Tasks.Add(new FleetWaypointTransportTask(task.action, task.amount, cargoType));
        }

    }
}