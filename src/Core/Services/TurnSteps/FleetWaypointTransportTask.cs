using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// </summary>
    public struct FleetWaypointTransportTask
    {
        public readonly WaypointTaskTransportAction action;
        public readonly int amount;
        public readonly CargoType cargoType;

        public FleetWaypointTransportTask(WaypointTaskTransportAction action, int amount, CargoType cargoType)
        {
            this.action = action;
            this.amount = amount;
            this.cargoType = cargoType;
        }
    }
}