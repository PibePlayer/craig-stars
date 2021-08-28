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

        public override void ComputeAggregate(bool recompute = false)
        {
            if (Aggregate.Computed && !recompute)
            {
                return;
            }

            base.ComputeAggregate(recompute);

            Aggregate.Stargate = null;
            Aggregate.BasePacketSpeed = 0;
            Aggregate.SafePacketSpeed = 0;
            int numAdditionalMassDrivers = 0;

            foreach (var slot in Design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    // find the first massdriver and stargate
                    if (slot.HullComponent.PacketSpeed > 0)
                    {
                        // if we already have a massdriver at this speed, add an additional mass driver to up
                        // our speed
                        if (Aggregate.BasePacketSpeed == slot.HullComponent.PacketSpeed)
                        {
                            numAdditionalMassDrivers++;
                        }
                        Aggregate.BasePacketSpeed = Math.Max(Aggregate.BasePacketSpeed, slot.HullComponent.PacketSpeed);
                    }
                    if (Aggregate.Stargate == null && slot.HullComponent.SafeHullMass > 0)
                    {
                        Aggregate.Stargate = slot.HullComponent;
                    }
                }
            }

            Aggregate.SafePacketSpeed = Aggregate.BasePacketSpeed + numAdditionalMassDrivers;
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
