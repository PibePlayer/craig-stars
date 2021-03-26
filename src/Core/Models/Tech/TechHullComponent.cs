using System.Collections.Generic;
using System.ComponentModel;

namespace CraigStars
{
    public class TechHullComponent : Tech
    {
        public const int NoScanner = -1;

        public HullSlotType HullSlotType { get; set; }
        public int Mass { get; set; }
        public int Armor { get; set; }
        public int Shield { get; set; }
        public int Cloak { get; set; }
        public bool CloakUnarmedOnly { get; set; }
        public int TorpedoBonus { get; set; }
        public int InitiativeBonus { get; set; }
        public int TorpedoJamming { get; set; }
        public int ReduceMovement { get; set; }
        public int ReduceCloaking { get; set; }
        public int FuelBonus { get; set; }
        public int FuelRegenerationRate { get; set; }
        public bool ColonizationModule { get; set; }
        public bool OrbitalConstructionModule { get; set; }
        public int CargoBonus { get; set; }
        public float MovementBonus { get; set; }
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
        public int MineSweep { get; set; }
        public bool Gattling { get; set; }
        public bool HitsAllTargets { get; set; }
        public bool DamageShieldsOnly { get; set; }
        public bool CapitalShipMissile { get; set; }

        // other
        public int PacketSpeed { get; set; }
        public int SafeHullMass { get; set; }
        public int SafeRange { get; set; }
        public int MaxHullMass { get; set; }
        public int MaxRange { get; set; }
        public int MiningRate { get; set; }
        public int TerraformRate { get; set; }
        public int MineLayingRate { get; set; }
        public int MaxSpeed { get; set; }
        public int ChanceOfHit { get; set; }
        public int DamagePerEngine { get; set; }
        public int DamagePerEngineRS { get; set; }
        public int MinDamagePerFleet { get; set; }
        public int MinDamagePerFleetRS { get; set; }


        public TechHullComponent() { }

        public TechHullComponent(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

    }
}
