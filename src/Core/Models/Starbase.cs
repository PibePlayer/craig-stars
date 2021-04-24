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
        /// Starbases don't have fuel
        /// </summary>
        /// <param name="warpFactor"></param>
        /// <param name="mass"></param>
        /// <param name="dist"></param>
        /// <param name="ifeFactor"></param>
        /// <param name="engine"></param>
        /// <returns></returns>
        internal override int GetFuelCost(int warpFactor, int mass, double dist, double ifeFactor, TechEngine engine)
        {
            return 0;
        }

        public override void ComputeAggregate(bool recompute = false)
        {
            if (Aggregate.Computed && !recompute)
            {
                return;
            }

            base.ComputeAggregate(recompute);

            Aggregate.Stargate = null;
            Aggregate.MassDriver = null;

            foreach (var slot in Design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    // find the first massdriver and stargate
                    if (Aggregate.MassDriver == null && slot.HullComponent.PacketSpeed > 0)
                    {
                        Aggregate.MassDriver = slot.HullComponent;
                    }
                    if (Aggregate.Stargate == null && slot.HullComponent.SafeHullMass > 0)
                    {
                        Aggregate.Stargate = slot.HullComponent;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the cost to upgrade an existing starbase to a new design
        /// </summary>
        /// <param name="design"></param>
        /// <returns></returns>
        public Cost GetUpgradeCost(ShipDesign design)
        {
            // TODO: Do Better
            return design.Aggregate.Cost - Tokens[0].Design.Aggregate.Cost;
        }

    }
}
