using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// If a waypoint has Transport as the task, we can specify a different task per
    /// mineral type
    /// </summary>
    public readonly struct WaypointTransportTask
    {
        public readonly WaypointTaskTransportAction action;
        public readonly int amount;

        [JsonConstructor]
        public WaypointTransportTask(WaypointTaskTransportAction action = WaypointTaskTransportAction.None, int amount = 0)
        {
            this.action = action;
            this.amount = amount;
        }
    }
}