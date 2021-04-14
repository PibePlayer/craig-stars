using System;

namespace CraigStars
{
    /// <summary>
    /// A type of hull slot| as well as helper functions for sets of hull slot types(like General)
    /// </summary>
    [Flags]
    public enum HullSlotType
    {
        None = 0,
        Engine = 1,
        Scanner = 2,
        Mechanical = 4,
        Bomb = 8,
        Mining = 16,
        Electrical = 32,
        Shield = 64,
        Armor = 128,
        Cargo = 256,
        SpaceDock = 512,
        Weapon = 1024,
        Orbital = 2048,
        Mine = 4096,
        OrbitalElectrical = HullSlotType.Orbital | HullSlotType.Electrical,
        ShieldElectricalMechanical = HullSlotType.Shield | HullSlotType.Electrical | HullSlotType.Mechanical,
        ScannerElectricalMechanical = HullSlotType.Scanner | HullSlotType.Electrical | HullSlotType.Mechanical,
        MineElectricalMechanical = HullSlotType.Mine | HullSlotType.Electrical | HullSlotType.Mechanical,
        ShieldArmor = HullSlotType.Shield | HullSlotType.Armor,
        WeaponShield = HullSlotType.Shield | HullSlotType.Weapon,
        General = HullSlotType.Scanner | HullSlotType.Mechanical | HullSlotType.Electrical | HullSlotType.Shield | HullSlotType.Armor | HullSlotType.Weapon | HullSlotType.Mine,
    }
}
