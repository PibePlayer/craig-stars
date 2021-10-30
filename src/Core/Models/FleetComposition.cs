using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Players (and AIs) use FleetCompositions to determine what they want a fleet to look like.
    /// </summary>
    public class FleetComposition
    {
        public Guid Guid { get; set; } = new();
        public FleetCompositionType Type { get; set; } = FleetCompositionType.None;
        public List<FleetCompositionToken> Tokens { get; set; } = new();

        /// <summary>
        /// Get a dictionary of FleetCompositionTokens for each ShipDesignPurpose this FleetComposition requires, keyed by ShipDesignPurpose
        /// </summary>
        /// <returns></returns>
        public Dictionary<ShipDesignPurpose, FleetCompositionToken> GetQuantityByPurpose() => Tokens.ToLookup(t => t.Purpose).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
    }
}