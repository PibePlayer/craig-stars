using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// During waypoint processing, we process waypoints in groups, i.e.
    /// all unloads, then all colonizations, etc. The FleetWaypoint
    /// is used in processing
    /// </summary>
    public class FleetWaypointProcessTask
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
        public List<FleetWaypointTransportTask> TransportTasks { get; set; } = new List<FleetWaypointTransportTask>();

        public FleetWaypointProcessTask(Fleet fleet, Waypoint waypoint, Player player)
        {
            Fleet = fleet;
            Waypoint = waypoint;
            Player = player;
        }

        public FleetWaypointProcessTask(Fleet fleet, Waypoint waypoint, List<FleetWaypointTransportTask> tasks)
        {
            Fleet = fleet;
            Waypoint = waypoint;
            TransportTasks = tasks;
        }

        public void Deconstruct(out Fleet fleet, out Waypoint waypoint, out Player player)
        {
            fleet = Fleet;
            waypoint = Waypoint;
            player = Player;
        }

        public void Deconstruct(out Fleet fleet, out Waypoint waypoint, out Player player, out List<FleetWaypointTransportTask> tasks)
        {
            fleet = Fleet;
            waypoint = Waypoint;
            player = Player;
            tasks = TransportTasks;
        }

        public void AddTask(WaypointTransportTask task, CargoType cargoType)
        {
            TransportTasks.Add(new FleetWaypointTransportTask(task.action, task.amount, cargoType));
        }

    }
}