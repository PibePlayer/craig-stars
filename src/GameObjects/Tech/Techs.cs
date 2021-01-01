using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public static class Techs
    {
        public static List<Tech> AllTechs { get; set; } = new List<Tech>();

        #region Engines
        public static readonly TechEngine GalaxyScoop = new TechEngine("Galaxy Scoop", new Cost(4, 2, 9, 12), new TechRequirements(energy: 5, propulsion: 20, lrtsRequired: LRT.IFE, lrtsDenied: LRT.NRSE), 130)
        {
            Mass = 8,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                60
            }
        };

        public static readonly TechEngine QuickJump5 = new TechEngine("Quick Jump 5", new Cost(3, 0, 1, 3), new TechRequirements(), 20)
        {
            Mass = 4,
            FuelUsage = new int[] {
                0,
                25,
                100,
                100,
                100,
                180,
                500,
                800,
                900,
                1080
            }
        };

        public static readonly TechEngine LongHump6 = new TechEngine("Long Hump 6", new Cost(5, 0, 1, 6), new TechRequirements(propulsion: 3), 30)
        {
            Mass = 9,
            FuelUsage = new int[] {
                0,
                25,
                100,
                100,
                100,
                180,
                500,
                800,
                900,
                1080
            }
        };

        #endregion Engines

        #region Scanners
        public static readonly TechHullComponent BatScanner = new TechHullComponent("Bat Scanner", new Cost(1, 0, 1, 1), new TechRequirements(), 10, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 2,
            ScanRange = 1
        };
        public static readonly TechHullComponent RhinoScanner = new TechHullComponent("Rhino Scanner", new Cost(3, 0, 2, 3), new TechRequirements(electronics: 1), 20, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 5,
            ScanRange = 50
        };
        public static readonly TechHullComponent MoleScanner = new TechHullComponent("Mole Scanner", new Cost(2, 0, 2, 9), new TechRequirements(electronics: 4), 30, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,

            Mass = 2,
            ScanRange = 100
        };
        public static readonly TechHullComponent PickPocketScanner = new TechHullComponent("Pick Pocket Scanner", new Cost(8, 10, 6, 35), new TechRequirements(energy: 4, electronics: 4, biotechnology: 4, prtRequired: PRT.SS), 40, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 15,
            StealCargo = true,
            ScanRange = 80
        };
        public static readonly TechHullComponent PossumScanner = new TechHullComponent("Possum Scanner", new Cost(3, 0, 3, 18), new TechRequirements(electronics: 5), 50, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 3,
            ScanRange = 150
        };
        public static readonly TechHullComponent DNAScanner = new TechHullComponent("DNA Scanner", new Cost(1, 1, 1, 5), new TechRequirements(propulsion: 3, biotechnology: 6), 60, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 2,
            ScanRange = 125
        };
        public static readonly TechHullComponent ChameleonScanner = new TechHullComponent("Chameleon Scanner", new Cost(4, 6, 4, 25), new TechRequirements(energy: 3, electronics: 6, prtRequired: PRT.SS), 70, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 6,
            ScanRange = 160,
            Cloak = 2,
            ScanRangePen = 45
        };
        public static readonly TechHullComponent FerretScanner = new TechHullComponent("Ferret Scanner", new Cost(2, 0, 8, 36), new TechRequirements(energy: 3, electronics: 7, biotechnology: 2, lrtsDenied: LRT.NAS), 80, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,

            Mass = 6,
            ScanRange = 185,
            ScanRangePen = 50
        };
        public static readonly TechHullComponent RNAScanner = new TechHullComponent("RNA Scanner", new Cost(1, 1, 2, 20), new TechRequirements(propulsion: 5, biotechnology: 10), 90, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,

            Mass = 2,
            ScanRange = 230
        };
        public static readonly TechHullComponent GazelleScanner = new TechHullComponent("Gazelle Scanner", new Cost(4, 0, 5, 24), new TechRequirements(energy: 4, electronics: 8), 100, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 5,
            ScanRange = 225
        };
        public static readonly TechHullComponent DolphinScanner = new TechHullComponent("Dolphin Scanner", new Cost(5, 5, 10, 40), new TechRequirements(energy: 5, electronics: 10, biotechnology: 4, lrtsDenied: LRT.NAS), 110, TechCategory.Scanner)
        {

            HullSlotType = HullSlotType.Scanner,
            Mass = 4,
            ScanRange = 220,
            ScanRangePen = 100
        };
        public static readonly TechHullComponent CheetahScanner = new TechHullComponent("Cheetah Scanner", new Cost(3, 1, 13, 50), new TechRequirements(energy: 5, electronics: 11), 115, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 4,
            ScanRange = 275
        };
        public static readonly TechHullComponent EagleEyeScanner = new TechHullComponent("Eagle Eye Scanner", new Cost(3, 2, 21, 64), new TechRequirements(energy: 6, electronics: 14), 120, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 3,
            ScanRange = 335
        };
        public static readonly TechHullComponent ElephantScanner = new TechHullComponent("Elephant Scanner", new Cost(8, 5, 14, 70), new TechRequirements(energy: 6, electronics: 16, biotechnology: 7, lrtsDenied: LRT.NAS), 130, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 6,
            ScanRange = 300,
            ScanRangePen = 200
        };
        public static readonly TechHullComponent PeerlessScanner = new TechHullComponent("Peerless Scanner", new Cost(3, 2, 30, 90), new TechRequirements(energy: 7, electronics: 24), 140, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,

            Mass = 4,
            ScanRange = 500
        };
        public static readonly TechHullComponent RobberBaronScanner = new TechHullComponent("Robber Baron Scanner", new Cost(10, 10, 10, 90), new TechRequirements(energy: 10, electronics: 15, biotechnology: 10, prtRequired: PRT.SS), 160, TechCategory.Scanner)
        {
            HullSlotType = HullSlotType.Scanner,
            Mass = 20,
            StealCargo = true,
            ScanRange = 220,
            ScanRangePen = 120
        };

        #endregion

        #region Mechanical
        public static readonly TechHullComponent FuelTank = new TechHullComponent("Fuel Tank", new Cost(5, 0, 0, 4), new TechRequirements(), 0, TechCategory.Mechanical)
        {
            Mass = 3,
            FuelBonus = 250,
            HullSlotType = HullSlotType.Mechanical
        };

        #endregion

        #region ShipHulls

        public static readonly TechHull Scout = new TechHull("Scout", new Cost(4, 2, 4, 9), new TechRequirements(), 50, TechCategory.ShipHull)
        {
            Mass = 8,
            BuiltInScannerForJoaT = true,
            Armor = 20,
            Initiative = 1,
            FuelCapacity = 50,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true, 64, 96, 64, 64),
                new TechHullSlot(HullSlotType.Scanner, 1, false, 192, 96, 64, 64),
                new TechHullSlot(HullSlotType.General, 1, false, 128, 96, 64, 64)
            })
        };

        #endregion

        #region StarbaseHulls
        public static readonly TechHull SpaceStation = new TechHull("Space Station", new Cost(106, 71, 220, 528), new TechRequirements(), 20, TechCategory.StarbaseHull)
        {
            Mass = 0,
            Armor = 500,
            Initiative = 14,
            Starbase = true
        };

        #endregion

        #region Planetary Scanners

        public static readonly TechPlanetaryScanner Viewer50 = new TechPlanetaryScanner("Viewer 50", new Cost(10, 10, 70, 100), new TechRequirements(), 0, TechCategory.PlanetaryScanner)
        {
            ScanRange = 50,
            ScanRangePen = 0
        };

        #endregion

        static Techs()
        {
            AllTechs.AddRange(new Tech[] {
                // engines
                QuickJump5,
                LongHump6,
                GalaxyScoop,

                // scanners
                BatScanner,
                RhinoScanner,
                MoleScanner,
                PickPocketScanner,
                PossumScanner,
                DNAScanner,
                ChameleonScanner,
                FerretScanner,
                RNAScanner,
                GazelleScanner,
                DolphinScanner,
                CheetahScanner,
                EagleEyeScanner,
                ElephantScanner,
                PeerlessScanner,
                RobberBaronScanner
            });
        }
    }
}