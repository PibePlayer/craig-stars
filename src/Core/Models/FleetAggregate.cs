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

        // starbase fields
        // TODO: generisize these
        public bool HasMassDriver { get => PacketSpeed > 0; }
        public bool HasStargate { get => SafeHullMass != TechHullComponent.NoGate; }
        public int PacketSpeed { get; set; }
        public int SafeHullMass { get; set; } = TechHullComponent.NoGate;
        public int MaxHullMass { get; set; } = TechHullComponent.NoGate;
        public int SafeRange { get; set; } = TechHullComponent.NoGate;
        public int MaxRange { get; set; } = TechHullComponent.NoGate;

    }
}