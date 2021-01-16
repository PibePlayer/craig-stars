using System;
using CraigStars.Singletons;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Each WaypointTransport lets you specify different orders for different cargo types
    /// </summary>
    public class WaypointTransportTasks
    {

        public WaypointTransportTask Fuel { get; set; } = new WaypointTransportTask(CargoType.Fuel);
        public WaypointTransportTask Ironium { get; set; } = new WaypointTransportTask(CargoType.Ironium);
        public WaypointTransportTask Boranium { get; set; } = new WaypointTransportTask(CargoType.Boranium);
        public WaypointTransportTask Germanium { get; set; } = new WaypointTransportTask(CargoType.Germanium);
        public WaypointTransportTask Colonists { get; set; } = new WaypointTransportTask(CargoType.Colonists);

        public WaypointTransportTasks() { }

        public WaypointTransportTasks(WaypointTransportTasks tasks)
        {
            Fuel.Action = tasks.Fuel.Action;
            Ironium.Action = tasks.Ironium.Action;
            Boranium.Action = tasks.Boranium.Action;
            Germanium.Action = tasks.Germanium.Action;
            Colonists.Action = tasks.Colonists.Action;

            Fuel.Amount = tasks.Fuel.Amount;
            Ironium.Amount = tasks.Ironium.Amount;
            Boranium.Amount = tasks.Boranium.Amount;
            Germanium.Amount = tasks.Germanium.Amount;
            Colonists.Amount = tasks.Colonists.Amount;
        }

        public WaypointTransportTask this[CargoType field]
        {
            get
            {
                switch (field)
                {
                    case CargoType.Ironium:
                        return Ironium;
                    case CargoType.Boranium:
                        return Boranium;
                    case CargoType.Germanium:
                        return Germanium;
                    case CargoType.Colonists:
                        return Colonists;
                    case CargoType.Fuel:
                        return Fuel;
                    default:
                        throw new IndexOutOfRangeException($"Index {field} out of range for {this.GetType().ToString()}");
                }
            }
        }
    }
}