using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Salvage from battle or a scrapped ship
    /// </summary>
    [JsonObject(IsReference = true)]
    public class Wormhole : MapObject
    {
        public int ReportAge = Unexplored;

        /// <summary>
        /// Each wormhole has a destination. Players only know about one wormhole to start
        /// </summary>
        [JsonProperty(IsReference = true)]
        public Wormhole Destination { get; set; }
        public WormholeStability Stability { get; set; }
    }
}
