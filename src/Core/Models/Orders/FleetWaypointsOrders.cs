
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypointsOrders : PlayerObjectOrder
    {
        public Guid BattlePlanGuid { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new();

        /// <summary>
        /// shoiuld the waypoint orders be repeated?
        /// </summary>
        public bool RepeatOrders { get; set; }

        /// <summary>
        /// for fleet renames
        /// </summary>
        public string BaseName { get; set; }
    }
}