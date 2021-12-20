using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An immediate merge order (i.e. the client clicks a merge in the UI) merging one or more fleets
    /// into a source fleet
    /// </summary>
    public abstract class FleetOrder
    {
        /// <summary>
        /// The source fleet that this order acts upon
        /// </summary>
        /// <value></value>
        [JsonProperty(IsReference = true)]
        public Fleet Source { get; set; }
    }
}