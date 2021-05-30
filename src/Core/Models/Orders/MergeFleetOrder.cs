using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An immediate merge order (i.e. the client clicks a merge in the UI) merging one or more fleets
    /// into a source fleet
    /// </summary>
    [JsonObject(IsReference = true)]
    public class MergeFleetOrder : FleetOrder
    {
        [JsonProperty(ItemIsReference = true)]
        public List<Fleet> MergingFleets { get; set; }
    }
}