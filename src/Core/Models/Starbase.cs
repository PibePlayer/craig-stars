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

        [JsonProperty(IsReference = true)]
        public Planet MassDriverTarget { get; set; }

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
            base.ComputeAggregate(recompute);

            if (Aggregate.Computed && !recompute)
            {
                return;
            }

            Aggregate.PacketSpeed = 0;
            Aggregate.SafeHullMass = TechHullComponent.NoGate;
            Aggregate.MaxHullMass = TechHullComponent.NoGate;
            Aggregate.SafeRange = TechHullComponent.NoGate;
            Aggregate.MaxRange = TechHullComponent.NoGate;

            foreach (var slot in Design.Slots)
            {
                // take the best packet thrower
                Aggregate.PacketSpeed = (int)Math.Max(Aggregate.PacketSpeed, slot.HullComponent.PacketSpeed);

                // take the best gate
                // TODO: this would turn two any/something something/any gates into any/any gates...
                // maybe we should pick the best one for each situation, if it comes to it?
                Aggregate.SafeHullMass = (int)Math.Max(Aggregate.SafeHullMass, slot.HullComponent.SafeHullMass);
                Aggregate.MaxHullMass = (int)Math.Max(Aggregate.MaxHullMass, slot.HullComponent.MaxHullMass);
                Aggregate.SafeRange = (int)Math.Max(Aggregate.SafeRange, slot.HullComponent.SafeRange);
                Aggregate.MaxRange = (int)Math.Max(Aggregate.MaxRange, slot.HullComponent.MaxRange);
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
