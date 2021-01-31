using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Each WaypointTransport lets you specify different orders for different cargo types
    /// </summary>
    public struct WaypointTransportTasks
    {
        public readonly WaypointTransportTask Fuel;
        public readonly WaypointTransportTask Ironium;
        public readonly WaypointTransportTask Boranium;
        public readonly WaypointTransportTask Germanium;
        public readonly WaypointTransportTask Colonists;

        [JsonConstructor]
        public WaypointTransportTasks(
            WaypointTransportTask fuel,
            WaypointTransportTask ironium,
            WaypointTransportTask boranium,
            WaypointTransportTask germanium,
            WaypointTransportTask colonists
        )
        {
            this.Fuel = fuel;
            this.Ironium = ironium;
            this.Boranium = boranium;
            this.Germanium = germanium;
            this.Colonists = colonists;
        }

        public WaypointTransportTasks(
            WaypointTransportTask? fuel = null,
            WaypointTransportTask? ironium = null,
            WaypointTransportTask? boranium = null,
            WaypointTransportTask? germanium = null,
            WaypointTransportTask? colonists = null
            )
        {
            this.Fuel = fuel != null ? fuel.Value : new WaypointTransportTask();
            this.Ironium = ironium != null ? ironium.Value : new WaypointTransportTask();
            this.Boranium = boranium != null ? boranium.Value : new WaypointTransportTask();
            this.Germanium = germanium != null ? germanium.Value : new WaypointTransportTask();
            this.Colonists = colonists != null ? colonists.Value : new WaypointTransportTask();
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