using System.Collections.Generic;

namespace CraigStars
{
    public class TechHullComponent : Tech
    {
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
        public int CargoBonus { get; set; }
        public int MovementBonus { get; set; }
        public int BeamDefense { get; set; }
        public int BeamBonus { get; set; }
        public int ScanRange { get; set; }
        public int ScanRangePen { get; set; }
        public bool StealCargo { get; set; }
        public bool Radiating { get; set; }
        public bool Smart { get; set; }
        public int KillRate { get; set; }
        public int MinKillRate { get; set; }
        public int StructureKillRate { get; set; }
        public int UnterraformRate { get; set; }
        public int Power { get; set; }
        public int Range { get; set; }
        public int Initiative { get; set; }
        public int Accuracy { get; set; }
        public int MineSweep { get; set; }
        public bool HitsAllTargets { get; set; }
        public bool DamageShieldsOnly { get; set; }
        public bool CapitalShipMissle { get; set; }
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
    }
}
