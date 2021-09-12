using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Players can store transport plans to quickly apply a plan to a waypoint
    /// </summary>
    public class TransportPlan : PlayerPlan<TransportPlan>
    {
        public TransportPlan() : base() { }
        public TransportPlan(string name) : base(name)
        {
        }

        public WaypointTransportTasks Tasks { get; set; }

        /// <summary>
        /// Make a clone of this transport plan
        /// </summary>
        /// <returns></returns>
        public override TransportPlan Clone()
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