using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// During waypoint processing, we process waypoints in groups, i.e.
    /// all unloads, then all colonizations, etc. The FleetWaypoint
    /// is used in processing
    /// </summary>
    public class PlanetInvasion
    {
        public Fleet Fleet { get; set; }
        public Planet Planet { get; set; }
        public int ColonistsToDrop { get; set; }

    }
}