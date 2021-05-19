using System.Collections.Generic;
using System.ComponentModel;

namespace CraigStars
{
    public class TechHullComponent : Tech
    {
        public const int NoScanner = -1;
        public const int NoGate = -1;
        public const int InfinteGate = int.MaxValue;

        public HullSlotType HullSlotType { get; set; }
        public int Mass { get; set; }
        public int Armor { get; set; }
        public int Shield { get; set; }
        
        /// <summary>
        /// The number of "cloak units" per kT this cloak provides for cloaking
        /// freighters
        /// </summary>
        public int CloakUnits { get; set; }
        public bool CloakUnarmedOnly { get; set; }

        /// <summary>
        /// This cumulative bonus decreases innaccuracy of torpedos on the ship
        /// i.e. 75% accurate torpedo with two .3f torpedo bonus
        /// is 100 - ((100 - 75) x .7 * .7) = 88% accuracy
        /// </summary>
        public float TorpedoBonus { get; set; }
        public int InitiativeBonus { get; set; }
        public int TorpedoJamming { get; set; }
        public int ReduceMovement { get; set; }
        public bool ReduceCloaking { get; set; }
        public int FuelBonus { get; set; }
        public int FuelRegenerationRate { get; set; }
        public bool ColonizationModule { get; set; }
        public bool OrbitalConstructionModule { get; set; }
        public int CargoBonus { get; set; }
        public int MovementBonus { get; set; }
        public int BeamDefense { get; set; }
        public int BeamBonus { get; set; }
        [DefaultValue(NoScanner)]
        public int ScanRange { get; set; } = NoScanner;
        [DefaultValue(NoScanner)]
        public int ScanRangePen { get; set; } = NoScanner;
        public bool StealCargo { get; set; }
        public bool Radiating { get; set; }

        // bombs
        public bool Smart { get; set; }
        public float KillRate { get; set; }
        public int MinKillRate { get; set; }
        public float StructureDestroyRate { get; set; }
        public int UnterraformRate { get; set; }

        // beams/torpedos
        public int Power { get; set; }
        public int Range { get; set; }
        public int Initiative { get; set; }
        public int Accuracy { get; set; }
        public bool Gattling { get; set; }
        public bool HitsAllTargets { get; set; }
        public bool DamageShieldsOnly { get; set; }
        public bool CapitalShipMissile { get; set; }

        // remote mining
        public int MiningRate { get; set; }

        // mass driver
        public int PacketSpeed { get; set; }

        // gates
        public int SafeHullMass { get; set; } = NoGate;
        public int SafeRange { get; set; } = NoGate;
        public int MaxHullMass { get; set; } = NoGate;
        public int MaxRange { get; set; } = NoGate;

        // Remote Terraforming
        public int TerraformRate { get; set; }

        // minelayers
        public MineFieldType MineFieldType { get; set; }
        public int MineLayingRate { get; set; }

        public TechHullComponent() { }

        public TechHullComponent(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

    }
}
