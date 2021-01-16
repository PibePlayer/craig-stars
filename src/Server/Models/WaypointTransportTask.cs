using System;
using CraigStars.Singletons;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// If a waypoint has Transport as the task, we can specify a different task per
    /// mineral type
    /// </summary>
    public class WaypointTransportTask
    {
        public CargoType Type { get; set; }
        public WaypointTaskTransportAction Action { get; set; }
        public int Amount { get; set; }

        public WaypointTransportTask() { }

        public WaypointTransportTask(CargoType type, WaypointTaskTransportAction action = WaypointTaskTransportAction.None, int amount = 0)
        {
            Type = type;
            Action = action;
            Amount = amount;
        }
    }
}