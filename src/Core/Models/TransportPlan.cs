using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Players can store transport plans to quickly apply a plan to a waypoint
    /// </summary>
    public class TransportPlan
    {
        public TransportPlan() { }
        public TransportPlan(string name)
        {
            Name = name;
        }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public WaypointTransportTasks Tasks { get; set; }

        /// <summary>
        /// Make a clone of this transport plan
        /// </summary>
        /// <returns></returns>
        public TransportPlan Clone()
        {
            return new TransportPlan()
            {
                Guid = Guid,
                Name = Name,
                Tasks = Tasks
            };
        }
    }
}