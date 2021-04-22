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
        public Wormhole()
        {
            Name = "Wormhole";
        }

        public int ReportAge = Unexplored;

        /// <summary>
        /// Each wormhole has a destination. Players only know about one wormhole to start
        /// </summary>
        [JsonProperty(IsReference = true)]
        public Wormhole Destination { get; set; }

        /// <summary>
        /// The current stability of this wormhole
        /// </summary>
        public WormholeStability Stability { get; set; }

        /// <summary>
        /// The number of years this wormhole has been at this stabililty
        /// </summary>
        public int YearsAtStability { get; set; }
    }
}
