using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A starbase is just a fleet with one token
    /// </summary>
    public class Starbase : Fleet
    {
        public int DockCapacity { get => Tokens?[0]?.Design?.Hull?.SpaceDock ?? 0; }

        // TODO: maybe it's better to store packet targets with teh starbase? it's doing the flinging...
        // [JsonProperty(IsReference = true)]
        // public Planet MassDriverTarget { get; set; }

        [JsonIgnore]
        public ShipDesign Design { get => Tokens[0].Design; }

        /// <summary>
        /// Calculate the cost to upgrade an existing starbase to a new design
        /// </summary>
        /// <param name="design"></param>
        /// <returns></returns>
        public Cost GetUpgradeCost(ShipDesign design)
        {
            // TODO: Do Better
            return design.Spec.Cost - Tokens[0].Design.Spec.Cost;
        }

    }
}
