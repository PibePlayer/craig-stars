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

        /// <summary>
        /// The base packet speed a packet can be flung. If there are two MassDriver7's on a starbase, the 
        /// base speed is 7 and the safe speed is 8
        /// </summary>
        public int BasePacketSpeed { get; set; }

        /// <summary>
        /// The safe warp speed a packet can be flung, or 0 if no mass driver
        /// </summary>
        public int SafePacketSpeed { get; set; }

        public bool HasMassDriver { get => BasePacketSpeed > 0; }

        public bool HasStargate { get => Stargate != null && Stargate.SafeHullMass != TechHullComponent.NoGate; }
        public TechHullComponent Stargate { get; set; }

        /// <summary>
        /// The amount of base cloaked cargo this fleet has (i.e. the mass of all uncloaked ships)
        /// </summary>
        public int BaseCloakedCargo { get; set; }

        /// <summary>
        /// Whether this Fleet's composition is complete, or if it needs more tokens
        /// </summary>
        /// <value></value>
        public bool FleetCompositionComplete { get; set; }

        /// <summary>
        /// The tokens this fleet requires to have a complete FleetComposition
        /// </summary>
        /// <value></value>
        public List<FleetCompositionToken> FleetCompositionTokensRequired { get; set; }

    }
}