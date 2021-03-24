using System;
using System.Collections.Generic;

namespace CraigStars
{
    public static class ShipDesigns
    {
        public static ShipDesign Starbase = new ShipDesign()
        {
            Name = "Starbase",
            Hull = Techs.SpaceStation,
            HullSetNumber = 0,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.Laser, 2, 8),
                new ShipDesignSlot(Techs.MoleSkinShield, 3, 8),
                new ShipDesignSlot(Techs.Laser, 4, 8),
                new ShipDesignSlot(Techs.MoleSkinShield, 5, 8),
                new ShipDesignSlot(Techs.MoleSkinShield, 6, 8),
                new ShipDesignSlot(Techs.Laser, 8, 8),
                new ShipDesignSlot(Techs.Laser, 10, 8),
                new ShipDesignSlot(Techs.MoleSkinShield, 12, 8),
            }
        };

        public static ShipDesign LongRangeScount = new ShipDesign()
        {
            Name = "Long Range Scout",
            Hull = Techs.Scout,
            HullSetNumber = 0,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.FuelTank, 2, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 3, 1),
            }
        };

        public static ShipDesign ArmoredProbe = new ShipDesign()
        {
            Name = "Armed Probe",
            Hull = Techs.Scout,
            HullSetNumber = 4,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.XRayLaser, 2, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 3, 1),
            }
        };

        public static ShipDesign SantaMaria = new ShipDesign()
        {
            Name = "Santa Maria",
            Hull = Techs.ColonyShip,
            HullSetNumber = 2,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.ColonizationModule, 2, 1),
            }
        };

        public static ShipDesign Teamster = new ShipDesign()
        {
            Name = "Teamster",
            Hull = Techs.MediumFreighter,
            HullSetNumber = 0,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 2, 1),
                new ShipDesignSlot(Techs.Crobmnium, 3, 1),
            }
        };

        public static ShipDesign CottonPicker = new ShipDesign()
        {
            Name = "Cotton Picker",
            Hull = Techs.MiniMiner,
            HullSetNumber = 0,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 2, 1),
                new ShipDesignSlot(Techs.RoboMiniMiner, 3, 1),
                new ShipDesignSlot(Techs.RoboMiniMiner, 4, 1),
            }
        };

        public static ShipDesign StalwartDefender = new ShipDesign()
        {
            Name = "Stalwart Defender",
            Hull = Techs.Destroyer,
            HullSetNumber = 4,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 1, 1),
                new ShipDesignSlot(Techs.XRayLaser, 2, 1),
                new ShipDesignSlot(Techs.BetaTorpedo, 3, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 4, 1),
                new ShipDesignSlot(Techs.Crobmnium, 5, 2),
                new ShipDesignSlot(Techs.FuelTank, 6, 1),
                new ShipDesignSlot(Techs.BattleComputer, 7, 1),
            }
        };

    }
}