using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Aggregate of fleet data, compiled from ship design aggregates.
    /// </summary>
    public class FleetAggregate : ShipDesignAggregate
    {
        public HashSet<ShipDesignPurpose> Purposes = new HashSet<ShipDesignPurpose>();
        public int TotalShips { get; set; }

        public int MassEmpty { get; set; }

        // starbase fields
        // TODO: generisize these
        public bool HasMassDriver { get => MassDriver != null && MassDriver.PacketSpeed > 0; }
        public bool HasStargate { get => Stargate != null && Stargate.SafeHullMass != TechHullComponent.NoGate; }
        public TechHullComponent MassDriver { get; set; }
        public TechHullComponent Stargate { get; set; }

        /// <summary>
        /// The amount of base cloaked cargo this fleet has (i.e. the mass of all uncloaked ships)
        /// </summary>
        public int BaseCloakedCargo { get; set; }
    }
}