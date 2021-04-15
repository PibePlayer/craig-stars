using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Ideally, we would load techs from json on disk for mods and such
    /// but we need something to start with. This class has all the base techs
    /// as static readonly vars
    /// </summary>
    public static class Techs
    {
        public static List<Tech> AllTechs { get; set; } = new List<Tech>();

        #region Engines

        public static readonly TechEngine SettlersDelight = new TechEngine("Settler's Delight", new Cost(1, 0, 1, 2), new TechRequirements(prtRequired: PRT.HE), 10)
        {
            Mass = 2,
            IdealSpeed = 6,
            FreeSpeed = 6,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                150,
                275,
                480,
                576
            }

        };
        public static readonly TechEngine QuickJump5 = new TechEngine("Quick Jump 5", new Cost(3, 0, 1, 3), new TechRequirements(), 20)
        {
            Mass = 4,
            IdealSpeed = 5,
            FuelUsage = new int[] {
                0,
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
        public static readonly TechEngine FuelMizer = new TechEngine("Fuel Mizer", new Cost(8, 0, 0, 11), new TechRequirements(propulsion: 2, lrtsRequired: LRT.IFE), 30)
        {
            Mass = 6,
            IdealSpeed = 6,
            FreeSpeed = 4,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                35,
                120,
                175,
                235,
                360,
                420
            }

        };
        public static readonly TechEngine LongHump6 = new TechEngine("Long Hump 6", new Cost(5, 0, 1, 6), new TechRequirements(propulsion: 3), 40)
        {
            Mass = 9,
            IdealSpeed = 6,
            FuelUsage = new int[] {
                0,
                0,
                20,
                60,
                100,
                100,
                105,
                450,
                750,
                900,
                1080
            }
        };
        public static readonly TechEngine DaddyLongLegs7 = new TechEngine("Daddy Long Legs 7", new Cost(11, 0, 3, 12), new TechRequirements(propulsion: 5), 50)
        {
            Mass = 13,
            IdealSpeed = 7,
            FuelUsage = new int[] {
                0,
                0,
                20,
                60,
                70,
                100,
                100,
                110,
                600,
                750,
                900
            }

        };
        public static readonly TechEngine AlphaDrive8 = new TechEngine("Alpha Drive 8", new Cost(16, 0, 3, 28), new TechRequirements(propulsion: 7), 60)
        {
            Mass = 17,
            IdealSpeed = 8,
            FuelUsage = new int[] {
                0,
                0,
                15,
                50,
                60,
                70,
                100,
                100,
                115,
                700,
                840
            }

        };
        public static readonly TechEngine TransGalacticDrive = new TechEngine("Trans-Galactic Drive", new Cost(20, 20, 9, 50), new TechRequirements(propulsion: 9), 70)
        {
            Mass = 25,
            IdealSpeed = 9,
            FuelUsage = new int[] {
                0,
                0,
                15,
                35,
                45,
                55,
                70,
                80,
                90,
                100,
                120
            }

        };
        public static readonly TechEngine Interspace10 = new TechEngine("Interspace-10", new Cost(18, 25, 10, 60), new TechRequirements(propulsion: 11, lrtsRequired: LRT.NRSE), 80)
        {
            Mass = 25,
            IdealSpeed = 10,
            FuelUsage = new int[] {
                0,
                0,
                10,
                30,
                40,
                50,
                60,
                70,
                80,
                90,
                100
            }

        };
        public static readonly TechEngine TransStar10 = new TechEngine("Trans-Star 10", new Cost(3, 0, 3, 10), new TechRequirements(propulsion: 23), 90)
        {
            Mass = 5,
            IdealSpeed = 10,
            FuelUsage = new int[] {
                0,
                0,
                5,
                15,
                20,
                25,
                30,
                35,
                40,
                45,
                50
            }

        };
        public static readonly TechEngine RadiatingHydroRamScoop = new TechEngine("Radiating Hydro-Ram Scoop", new Cost(3, 2, 9, 8), new TechRequirements(energy: 2, propulsion: 6, lrtsDenied: LRT.NRSE), 100)
        {
            Mass = 10,
            Radiating = true,
            IdealSpeed = 6,
            FreeSpeed = 6,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                165,
                375,
                600,
                720
            }

        };
        public static readonly TechEngine SubGalacticFuelScoop = new TechEngine("Sub-Galactic Fuel Scoop", new Cost(4, 4, 7, 12), new TechRequirements(energy: 2, propulsion: 8, lrtsDenied: LRT.NRSE), 110)
        {
            Mass = 20,
            IdealSpeed = 7,
            FreeSpeed = 7,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                85,
                105,
                210,
                380,
                456
            }

        };

        public static readonly TechEngine TransGalacticFuelScoop = new TechEngine("Trans-Galactic Fuel Scoop", new Cost(5, 4, 12, 18), new TechRequirements(energy: 3, propulsion: 9, lrtsDenied: LRT.NRSE), 120)
        {
            Mass = 19,
            IdealSpeed = 8,
            FreeSpeed = 8,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                88,
                100,
                145,
                174
            }

        };
        public static readonly TechEngine TransGalacticSuperScoop = new TechEngine("Trans-Galactic Super Scoop", new Cost(6, 4, 16, 24), new TechRequirements(energy: 4, propulsion: 12, lrtsDenied: LRT.NRSE), 130)
        {
            Mass = 18,
            IdealSpeed = 9,
            FreeSpeed = 9,
            FuelUsage = new int[] {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                65,
                90,
                108
            }

        };
        public static readonly TechEngine TransGalacticMizerScoop = new TechEngine("Trans-Galactic Mizer Scoop", new Cost(5, 2, 13, 11), new TechRequirements(energy: 4, propulsion: 16, lrtsDenied: LRT.NRSE), 140)
        {
            Mass = 11,
            IdealSpeed = 10,
            FreeSpeed = 10,
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
                70,
                84
            }

        };
        public static readonly TechEngine GalaxyScoop = new TechEngine("Galaxy Scoop", new Cost(4, 2, 9, 12), new TechRequirements(energy: 5, propulsion: 20, lrtsRequired: LRT.IFE, lrtsDenied: LRT.NRSE), 150)
        {
            Mass = 8,
            IdealSpeed = 10,
            FreeSpeed = 10,
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
                0,
                60
            }
        };

        #endregion Engines

        #region Mass Drivers

        public static readonly TechHullComponent MassDriver5 = new TechHullComponent("Mass Driver 5", new Cost(24, 20, 20, 70), new TechRequirements(energy: 4, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 5,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent MassDriver6 = new TechHullComponent("Mass Driver 6", new Cost(24, 20, 20, 144), new TechRequirements(energy: 7, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 6,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent MassDriver7 = new TechHullComponent("Mass Driver 7", new Cost(100, 100, 100, 512), new TechRequirements(energy: 9), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 7,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent SuperDriver8 = new TechHullComponent("Super Driver 8", new Cost(24, 20, 20, 256), new TechRequirements(energy: 11, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 8,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent SuperDriver9 = new TechHullComponent("Super Driver 9", new Cost(24, 20, 20, 324), new TechRequirements(energy: 13, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 9,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent UltraDriver10 = new TechHullComponent("Ultra Driver 10", new Cost(100, 100, 100, 968), new TechRequirements(energy: 15), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 10,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent UltraDriver11 = new TechHullComponent("Ultra Driver 11", new Cost(24, 20, 20, 484), new TechRequirements(energy: 17, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 11,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent UltraDriver12 = new TechHullComponent("Ultra Driver 12", new Cost(24, 20, 20, 576), new TechRequirements(energy: 20, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 12,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent UltraDriver13 = new TechHullComponent("Ultra Driver 13", new Cost(24, 20, 20, 676), new TechRequirements(energy: 24, prtRequired: PRT.PP), 0, TechCategory.Orbital)
        {
            Mass = 0,
            PacketSpeed = 13,
            HullSlotType = HullSlotType.Orbital,
        };

        #endregion

        #region Stargates

        public static readonly TechHullComponent Stargate100_250 = new TechHullComponent("Stargate 100-250", new Cost(50, 20, 20, 200), new TechRequirements(propulsion: 5, construction: 5), 0, TechCategory.Orbital)
        {
            Mass = 0,
            MaxRange = 1250,
            SafeHullMass = 100,
            SafeRange = 250,
            MaxHullMass = 500,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent Stargate100_Any = new TechHullComponent("Stargate 100-any", new Cost(50, 20, 20, 700), new TechRequirements(propulsion: 16, construction: 12, prtRequired: PRT.IT), 0, TechCategory.Orbital)
        {
            Mass = 0,
            SafeHullMass = 100,
            MaxHullMass = 500,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent Stargate150_600 = new TechHullComponent("Stargate 150-600", new Cost(50, 20, 20, 500), new TechRequirements(propulsion: 11, construction: 7), 0, TechCategory.Orbital)
        {
            Mass = 0,
            MaxRange = 3000,
            SafeHullMass = 150,
            SafeRange = 600,
            MaxHullMass = 7,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent Stargate300_500 = new TechHullComponent("Stargate 300-500", new Cost(50, 20, 20, 600), new TechRequirements(propulsion: 9, construction: 13), 0, TechCategory.Orbital)
        {
            Mass = 0,
            MaxRange = 2500,
            SafeHullMass = 300,
            SafeRange = 500,
            MaxHullMass = 15,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent StargateAny_300 = new TechHullComponent("Stargate any-300", new Cost(50, 20, 20, 250), new TechRequirements(propulsion: 6, construction: 10, prtRequired: PRT.IT), 0, TechCategory.Orbital)
        {
            Mass = 0,
            MaxRange = 1500,
            SafeRange = 300,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent StargateAny_800 = new TechHullComponent("Stargate any-800", new Cost(50, 20, 20, 700), new TechRequirements(propulsion: 12, construction: 18, prtRequired: PRT.IT), 0, TechCategory.Orbital)
        {
            Mass = 0,
            MaxRange = 4000,
            SafeRange = 800,
            HullSlotType = HullSlotType.Orbital,
        };
        public static readonly TechHullComponent StargateAny_Any = new TechHullComponent("Stargate any-any", new Cost(50, 20, 20, 800), new TechRequirements(propulsion: 19, construction: 24, prtRequired: PRT.IT), 0, TechCategory.Orbital)
        {
            Mass = 0,
            HullSlotType = HullSlotType.Orbital,
        };

        #endregion

        #region Miners

        public static readonly TechHullComponent OrbitalAdjuster = new TechHullComponent("Orbital Adjuster", new Cost(25, 25, 25, 50), new TechRequirements(biotechnology: 6, prtRequired: PRT.CA), 0, TechCategory.MineRobot)
        {
            Mass = 80,
            Cloak = 25,
            TerraformRate = 1,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboMiner = new TechHullComponent("Robo-Miner", new Cost(30, 0, 7, 100), new TechRequirements(construction: 4, electronics: 2, lrtsDenied: LRT.OBRM), 0, TechCategory.MineRobot)
        {
            Mass = 240,
            MiningRate = 12,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboMaxiMiner = new TechHullComponent("Robo-Maxi-Miner", new Cost(30, 0, 7, 100), new TechRequirements(construction: 7, electronics: 4, lrtsDenied: LRT.OBRM), 0, TechCategory.MineRobot)
        {
            Mass = 240,
            MiningRate = 18,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboMidgetMiner = new TechHullComponent("Robo-Midget-Miner", new Cost(12, 0, 4, 44), new TechRequirements(lrtsRequired: LRT.ARM), 0, TechCategory.MineRobot)
        {
            Mass = 80,
            MiningRate = 5,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboMiniMiner = new TechHullComponent("Robo-Mini-Miner", new Cost(29, 0, 7, 96), new TechRequirements(construction: 2, electronics: 1), 0, TechCategory.MineRobot)
        {
            Mass = 240,
            MiningRate = 4,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboSuperMiner = new TechHullComponent("Robo-Super-Miner", new Cost(30, 0, 7, 100), new TechRequirements(construction: 12, electronics: 6, lrtsDenied: LRT.OBRM), 0, TechCategory.MineRobot)
        {
            Mass = 240,
            MiningRate = 27,
            HullSlotType = HullSlotType.Mining,
        };
        public static readonly TechHullComponent RoboUltraMiner = new TechHullComponent("Robo-Ultra-Miner", new Cost(14, 0, 4, 100), new TechRequirements(construction: 15, electronics: 8, lrtsRequired: LRT.ARM, lrtsDenied: LRT.OBRM), 0, TechCategory.MineRobot)
        {
            Mass = 80,
            MiningRate = 25,
            HullSlotType = HullSlotType.Mining,
        };

        #endregion

        #region Bombs

        public static readonly TechHullComponent LadyFingerBomb = new TechHullComponent("Lady Finger Bomb", new Cost(1, 19, 0, 5), new TechRequirements(weapons: 2), 0, TechCategory.Bomb)
        {
            Mass = 40,
            MinKillRate = 300,
            StructureDestroyRate = .2f,
            KillRate = .6f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent BlackCatBomb = new TechHullComponent("Black Cat Bomb", new Cost(1, 22, 0, 7), new TechRequirements(weapons: 5), 10, TechCategory.Bomb)
        {
            Mass = 45,
            MinKillRate = 300,
            StructureDestroyRate = .4f,
            KillRate = .9f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent M70Bomb = new TechHullComponent("M-70 Bomb", new Cost(1, 24, 0, 9), new TechRequirements(weapons: 8), 20, TechCategory.Bomb)
        {
            Mass = 50,
            MinKillRate = 300,
            StructureDestroyRate = .6f,
            KillRate = 1.2f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent M80Bomb = new TechHullComponent("M-80 Bomb", new Cost(1, 25, 0, 12), new TechRequirements(weapons: 11), 30, TechCategory.Bomb)
        {
            Mass = 55,
            MinKillRate = 300,
            StructureDestroyRate = .7f,
            KillRate = 1.7f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent CherryBomb = new TechHullComponent("Cherry Bomb", new Cost(1, 25, 0, 11), new TechRequirements(weapons: 14), 40, TechCategory.Bomb)
        {
            Mass = 52,
            MinKillRate = 300,
            StructureDestroyRate = 1.0f,
            KillRate = 2.5f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent LBU17Bomb = new TechHullComponent("LBU-17 Bomb", new Cost(1, 15, 15, 7), new TechRequirements(weapons: 5, electronics: 8), 50, TechCategory.Bomb)
        {
            Mass = 30,
            StructureDestroyRate = 1.6f,
            KillRate = .2f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent LBU32Bomb = new TechHullComponent("LBU-32 Bomb", new Cost(1, 24, 15, 10), new TechRequirements(weapons: 10, electronics: 10), 60, TechCategory.Bomb)
        {
            Mass = 35,
            StructureDestroyRate = 2.8f,
            KillRate = .3f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent LBU74Bomb = new TechHullComponent("LBU-74 Bomb", new Cost(1, 33, 12, 14), new TechRequirements(weapons: 15, electronics: 12), 70, TechCategory.Bomb)
        {
            Mass = 45,
            StructureDestroyRate = 4.5f,
            KillRate = .4f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent RetroBomb = new TechHullComponent("Retro Bomb", new Cost(15, 15, 10, 50), new TechRequirements(weapons: 10, biotechnology: 12, prtRequired: PRT.CA), 80, TechCategory.Bomb)
        {
            Mass = 45,
            UnterraformRate = 1,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent SmartBomb = new TechHullComponent("Smart Bomb", new Cost(1, 22, 0, 27), new TechRequirements(weapons: 5, biotechnology: 7), 90, TechCategory.Bomb)
        {
            Mass = 50,
            Smart = true,
            KillRate = 1.3f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent NeutronBomb = new TechHullComponent("Neutron Bomb", new Cost(1, 30, 0, 30), new TechRequirements(weapons: 10, biotechnology: 10), 110, TechCategory.Bomb)
        {
            Mass = 57,
            Smart = true,
            KillRate = 2.2f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent EnrichedNeutronBomb = new TechHullComponent("Enriched Neutron Bomb", new Cost(1, 36, 0, 25), new TechRequirements(weapons: 15, biotechnology: 12), 120, TechCategory.Bomb)
        {
            Mass = 64,
            Smart = true,
            KillRate = 3.5f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent PeerlessBomb = new TechHullComponent("Peerless Bomb", new Cost(1, 33, 0, 32), new TechRequirements(weapons: 22, biotechnology: 15), 130, TechCategory.Bomb)
        {
            Mass = 55,
            Smart = true,
            KillRate = 5.0f,
            HullSlotType = HullSlotType.Bomb
        };
        public static readonly TechHullComponent AnnihilatorBomb = new TechHullComponent("Annihilator Bomb", new Cost(1, 30, 0, 28), new TechRequirements(weapons: 26, biotechnology: 17), 140, TechCategory.Bomb)
        {
            Mass = 50,
            Smart = true,
            KillRate = 7.0f,
            HullSlotType = HullSlotType.Bomb
        };

        #endregion

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

        #region Armor

        public static readonly TechHullComponent Tritanium = new TechHullComponent("Tritanium", new Cost(5, 0, 0, 9), new TechRequirements(), 10, TechCategory.Armor)
        {
            Mass = 60,
            Armor = 50,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Crobmnium = new TechHullComponent("Crobmnium", new Cost(6, 0, 0, 13), new TechRequirements(construction: 3), 20, TechCategory.Armor)
        {
            Mass = 56,
            Armor = 75,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Carbonic = new TechHullComponent("Carbonic Armor", new Cost(5, 0, 0, 15), new TechRequirements(biotechnology: 4), 30, TechCategory.Armor)
        {
            Mass = 25,
            Armor = 100,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Strobnium = new TechHullComponent("Strobnium", new Cost(8, 0, 0, 18), new TechRequirements(construction: 6), 40, TechCategory.Armor)
        {
            Mass = 54,
            Armor = 120,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Organic = new TechHullComponent("Organic Armor", new Cost(0, 0, 6, 20), new TechRequirements(biotechnology: 7), 50, TechCategory.Armor)
        {
            Mass = 15,
            Armor = 175,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Kelarium = new TechHullComponent("Kelarium", new Cost(9, 1, 0, 25), new TechRequirements(construction: 9), 60, TechCategory.Armor)
        {
            Mass = 50,
            Armor = 180,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Fielded = new TechHullComponent("Fielded Kelarium", new Cost(10, 0, 2, 28), new TechRequirements(energy: 4, construction: 10, prtRequired: PRT.IS), 70, TechCategory.Armor)
        {
            Mass = 50,
            Shield = 50,
            Armor = 175,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent DepletedNeutronium = new TechHullComponent("Depleted Neutronium", new Cost(10, 0, 2, 28), new TechRequirements(construction: 10, electronics: 3, prtRequired: PRT.SS), 80, TechCategory.Armor)
        {
            Mass = 50,
            Armor = 200,
            Cloak = 25,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Neutronium = new TechHullComponent("Neutronium", new Cost(11, 2, 1, 30), new TechRequirements(construction: 12), 90, TechCategory.Armor)
        {
            Mass = 45,
            Armor = 275,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Valanium = new TechHullComponent("Valanium", new Cost(15, 0, 0, 50), new TechRequirements(construction: 16), 100, TechCategory.Armor)
        {
            Mass = 40,
            Armor = 500,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Superlatanium = new TechHullComponent("Superlatanium", new Cost(25, 0, 0, 100), new TechRequirements(construction: 24), 110, TechCategory.Armor)
        {
            Mass = 30,
            Armor = 1500,
            HullSlotType = HullSlotType.Armor
        };
        #endregion

        #region Electronics

        #region Cloaks

        public static readonly TechHullComponent TransportCloaking = new TechHullComponent("Transport Cloaking", new Cost(2, 0, 2, 3), new TechRequirements(prtRequired: PRT.SS), 0, TechCategory.Electrical)
        {
            Mass = 1,
            CloakUnarmedOnly = true,
            Cloak = 75,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent StealthCloak = new TechHullComponent("Stealth Cloak", new Cost(2, 0, 2, 5), new TechRequirements(energy: 2, electronics: 5), 10, TechCategory.Electrical)
        {
            Mass = 2,
            Cloak = 35,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent SuperStealthCloak = new TechHullComponent("Super-Stealth Cloak", new Cost(8, 0, 8, 15), new TechRequirements(energy: 4, electronics: 10), 20, TechCategory.Electrical)
        {
            Mass = 3,
            Cloak = 55,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent UltraStealthCloak = new TechHullComponent("Ultra-Stealth Cloak", new Cost(10, 0, 10, 25), new TechRequirements(energy: 10, electronics: 12, prtRequired: PRT.SS), 30, TechCategory.Electrical)
        {
            Mass = 5,
            Cloak = 85,
            HullSlotType = HullSlotType.Electrical,
        };

        #endregion

        public static readonly TechHullComponent BattleComputer = new TechHullComponent("Battle Computer", new Cost(0, 0, 13, 5), new TechRequirements(), 40, TechCategory.Electrical)
        {
            Mass = 1,
            InitiativeBonus = 1,
            TorpedoBonus = .2f,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent BattleSuperComputer = new TechHullComponent("Battle Super Computer", new Cost(0, 0, 25, 14), new TechRequirements(energy: 5, electronics: 11), 50, TechCategory.Electrical)
        {
            Mass = 1,
            InitiativeBonus = 2,
            TorpedoBonus = .3f,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent BattleNexus = new TechHullComponent("Battle Nexus", new Cost(0, 0, 30, 15), new TechRequirements(energy: 10, electronics: 19), 60, TechCategory.Electrical)
        {
            Mass = 1,
            InitiativeBonus = 3,
            TorpedoBonus = .5f,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent Jammer10 = new TechHullComponent("Jammer 10", new Cost(0, 0, 2, 6), new TechRequirements(energy: 2, electronics: 6, prtRequired: PRT.IS), 70, TechCategory.Electrical)
        {
            Mass = 1,
            TorpedoJamming = 1,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent Jammer20 = new TechHullComponent("Jammer 20", new Cost(1, 0, 5, 20), new TechRequirements(energy: 4, electronics: 10), 80, TechCategory.Electrical)
        {
            Mass = 1,
            TorpedoJamming = 2,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent Jammer30 = new TechHullComponent("Jammer 30", new Cost(1, 0, 6, 20), new TechRequirements(energy: 8, electronics: 16), 90, TechCategory.Electrical)
        {
            Mass = 1,
            TorpedoJamming = 3,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent Jammer50 = new TechHullComponent("Jammer 50", new Cost(2, 0, 7, 20), new TechRequirements(energy: 16, electronics: 22), 100, TechCategory.Electrical)
        {
            Mass = 1,
            TorpedoJamming = 5,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent EnergyCapacitor = new TechHullComponent("Energy Capacitor", new Cost(0, 0, 8, 5), new TechRequirements(energy: 7, electronics: 4), 110, TechCategory.Electrical)
        {
            Mass = 1,
            BeamBonus = 1,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent FluxCapacitor = new TechHullComponent("Flux Capacitor", new Cost(0, 0, 8, 5), new TechRequirements(energy: 14, electronics: 8, prtRequired: PRT.HE), 120, TechCategory.Electrical)
        {
            Mass = 1,
            BeamBonus = 1,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent EnergyDampener = new TechHullComponent("Energy Dampener", new Cost(5, 10, 0, 50), new TechRequirements(energy: 14, propulsion: 8, prtRequired: PRT.SD), 130, TechCategory.Electrical)
        {
            Mass = 2,
            ReduceMovement = 1,
            HullSlotType = HullSlotType.Electrical,
        };

        public static readonly TechHullComponent TachyonDetector = new TechHullComponent("Tachyon Detector", new Cost(1, 5, 0, 70), new TechRequirements(energy: 8, electronics: 14, prtRequired: PRT.IS), 140, TechCategory.Electrical)
        {
            Mass = 1,
            ReduceCloaking = 5,
            HullSlotType = HullSlotType.Electrical,
        };

        #endregion

        #region MysteryTrader

        public static readonly TechHullComponent AntiMatterGenerator = new TechHullComponent("Anti-Matter Generator", new Cost(8, 3, 3, 10), new TechRequirements(weapons: 12, biotechnology: 7, prtRequired: PRT.IT), 150, TechCategory.Electrical)
        {
            Mass = 10,
            FuelRegenerationRate = 50,
            FuelBonus = 200,
            HullSlotType = HullSlotType.Electrical,
        };

        #endregion


        #region Mine Layers

        public static readonly TechHullComponent MineDispenser40 = new TechHullComponent("Mine Dispenser 40", new Cost(2, 9, 7, 40), new TechRequirements(prtRequired: PRT.SD), 0, TechCategory.MineLayer)
        {
            Mass = 25,
            MineFieldType = MineFieldType.Normal,
            MineLayingRate = 40,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent MineDispenser50 = new TechHullComponent("Mine Dispenser 50", new Cost(2, 12, 10, 55), new TechRequirements(energy: 2, biotechnology: 4), 10, TechCategory.MineLayer)
        {
            Mass = 30,
            MineFieldType = MineFieldType.Normal,
            MineLayingRate = 50,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent MineDispenser80 = new TechHullComponent("Mine Dispenser 80", new Cost(2, 12, 10, 65), new TechRequirements(energy: 3, biotechnology: 7, prtRequired: PRT.SD), 20, TechCategory.MineLayer)
        {
            Mass = 30,
            MineFieldType = MineFieldType.Normal,
            MineLayingRate = 80,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent MineDispenser130 = new TechHullComponent("Mine Dispenser 130", new Cost(2, 18, 10, 80), new TechRequirements(energy: 6, biotechnology: 12, prtRequired: PRT.SD), 30, TechCategory.MineLayer)
        {
            Mass = 30,
            MineFieldType = MineFieldType.Normal,
            MineLayingRate = 130,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent HeavyDispenser50 = new TechHullComponent("Heavy Dispenser 50", new Cost(2, 20, 5, 50), new TechRequirements(energy: 5, biotechnology: 3, prtRequired: PRT.SD), 40, TechCategory.MineLayer)
        {
            Mass = 10,
            MineFieldType = MineFieldType.Heavy,
            MineLayingRate = 50,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent HeavyDispenser110 = new TechHullComponent("Heavy Dispenser 110", new Cost(2, 20, 5, 50), new TechRequirements(energy: 9, biotechnology: 5, prtRequired: PRT.SD), 50, TechCategory.MineLayer)
        {
            Mass = 15,
            MineFieldType = MineFieldType.Heavy,
            MineLayingRate = 110,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent HeavyDispenser200 = new TechHullComponent("Heavy Dispenser 200", new Cost(2, 45, 5, 90), new TechRequirements(energy: 14, biotechnology: 7, prtRequired: PRT.SD), 60, TechCategory.MineLayer)
        {
            Mass = 20,
            MineFieldType = MineFieldType.Heavy,
            MineLayingRate = 200,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent SpeedTrap20 = new TechHullComponent("Speed Trap 20", new Cost(29, 0, 12, 58), new TechRequirements(propulsion: 2, biotechnology: 2, prtRequired: PRT.IS), 70, TechCategory.MineLayer)
        {
            Mass = 100,
            MineFieldType = MineFieldType.Speed,
            MineLayingRate = 20,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent SpeedTrap30 = new TechHullComponent("Speed Trap 30", new Cost(32, 0, 14, 72), new TechRequirements(propulsion: 3, biotechnology: 6, prtRequired: PRT.IS), 80, TechCategory.MineLayer)
        {
            Mass = 135,
            MineFieldType = MineFieldType.Speed,
            MineLayingRate = 30,
            HullSlotType = HullSlotType.Mine,
        };

        public static readonly TechHullComponent SpeedTrap50 = new TechHullComponent("Speed Trap 50", new Cost(40, 0, 15, 80), new TechRequirements(propulsion: 5, biotechnology: 11, prtRequired: PRT.IS), 90, TechCategory.MineLayer)
        {
            Mass = 140,
            MineFieldType = MineFieldType.Speed,
            MineLayingRate = 50,
            HullSlotType = HullSlotType.Mine,
        };

        #endregion

        #region Mechanical

        public static readonly TechHullComponent ColonizationModule = new TechHullComponent("Colonization Module", new Cost(11, 9, 9, 9), new TechRequirements(), 0, TechCategory.Mechanical)
        {
            Mass = 32,
            ColonizationModule = true,
            HullSlotType = HullSlotType.Mechanical
        };
        public static readonly TechHullComponent OrbitalConstructionModule = new TechHullComponent("Orbital Construction Module", new Cost(18, 13, 13, 18), new TechRequirements(prtRequired: PRT.AR), 10, TechCategory.Mechanical)
        {
            Mass = 50,
            MinKillRate = 2000,
            OrbitalConstructionModule = true,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent CargoPod = new TechHullComponent("Cargo Pod", new Cost(5, 0, 2, 10), new TechRequirements(construction: 3), 20, TechCategory.Mechanical)
        {
            Mass = 5,
            CargoBonus = 50,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent SuperCargoPod = new TechHullComponent("Super Cargo Pod", new Cost(8, 0, 2, 15), new TechRequirements(energy: 3, construction: 8), 30, TechCategory.Mechanical)
        {
            Mass = 7,
            CargoBonus = 100,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent FuelTank = new TechHullComponent("Fuel Tank", new Cost(5, 0, 0, 4), new TechRequirements(), 40, TechCategory.Mechanical)
        {
            Mass = 3,
            FuelBonus = 250,
            HullSlotType = HullSlotType.Mechanical
        };
        public static readonly TechHullComponent SuperFuelTank = new TechHullComponent("Super Fuel Tank", new Cost(8, 0, 0, 8), new TechRequirements(energy: 6, propulsion: 4, construction: 14), 50, TechCategory.Mechanical)
        {
            Mass = 8,
            FuelBonus = 500,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent ManeuveringJet = new TechHullComponent("Maneuvering Jet", new Cost(5, 0, 5, 10), new TechRequirements(energy: 2, propulsion: 3), 60, TechCategory.Mechanical)
        {
            Mass = 5,
            MovementBonus = 1,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent Overthruster = new TechHullComponent("Overthruster", new Cost(10, 0, 8, 20), new TechRequirements(energy: 5, propulsion: 12), 70, TechCategory.Mechanical)
        {
            Mass = 5,
            MovementBonus = 2,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent BeamDeflector = new TechHullComponent("Beam Deflector", new Cost(0, 0, 10, 8), new TechRequirements(energy: 6, weapons: 6, construction: 6, electronics: 6), 80, TechCategory.Mechanical)
        {
            Mass = 1,
            HullSlotType = HullSlotType.Mechanical,
            BeamDefense = 1
        };

        #endregion

        #region BeamWeapons

        public static readonly TechHullComponent Laser = new TechHullComponent("Laser", new Cost(0, 5, 0, 4), new TechRequirements(), 0, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 10,
            HullSlotType = HullSlotType.Weapon,

            Range = 1
        };
        public static readonly TechHullComponent XRayLaser = new TechHullComponent("X-Ray Laser", new Cost(0, 6, 0, 6), new TechRequirements(weapons: 3), 10, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 16,
            HullSlotType = HullSlotType.Weapon,

            Range = 1
        };
        public static readonly TechHullComponent MiniGun = new TechHullComponent("Mini Gun", new Cost(0, 6, 0, 6), new TechRequirements(weapons: 5, prtRequired: PRT.IS), 20, TechCategory.BeamWeapon)
        {
            Mass = 3,
            Initiative = 12,
            MineSweep = 208,
            Power = 16,
            HitsAllTargets = true,
            HullSlotType = HullSlotType.Weapon,

            Range = 2
        };
        public static readonly TechHullComponent YakimoraLightPhaser = new TechHullComponent("Yakimora Light Phaser", new Cost(0, 8, 0, 7), new TechRequirements(weapons: 6), 30, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 26,
            HullSlotType = HullSlotType.Weapon,

            Range = 1
        };
        public static readonly TechHullComponent Blackjack = new TechHullComponent("Blackjack", new Cost(0, 16, 0, 7), new TechRequirements(weapons: 7), 40, TechCategory.BeamWeapon)
        {
            Mass = 10,
            Initiative = 10,
            Power = 90,
            HullSlotType = HullSlotType.Weapon,

            Range = 0
        };
        public static readonly TechHullComponent PhaserBazooka = new TechHullComponent("Phaser Bazooka", new Cost(0, 8, 0, 11), new TechRequirements(weapons: 8), 50, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 7,
            Power = 26,
            HullSlotType = HullSlotType.Weapon,

            Range = 2
        };
        public static readonly TechHullComponent PulsedSapper = new TechHullComponent("Pulsed Sapper", new Cost(0, 0, 4, 12), new TechRequirements(energy: 5, weapons: 9), 60, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 14,
            DamageShieldsOnly = true,
            Power = 82,
            HullSlotType = HullSlotType.Weapon,

            Range = 3
        };
        public static readonly TechHullComponent ColloidalPhaser = new TechHullComponent("Colloidal Phaser", new Cost(0, 14, 0, 18), new TechRequirements(weapons: 10), 70, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 5,
            Power = 26,
            HullSlotType = HullSlotType.Weapon,

            Range = 3
        };
        public static readonly TechHullComponent GatlingGun = new TechHullComponent("Gatling Gun", new Cost(0, 20, 0, 13), new TechRequirements(weapons: 11), 80, TechCategory.BeamWeapon)
        {
            Mass = 3,
            Initiative = 12,
            MineSweep = 496,
            Power = 31,
            HitsAllTargets = true,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        public static readonly TechHullComponent MiniBlaster = new TechHullComponent("Mini Blaster", new Cost(0, 10, 0, 9), new TechRequirements(weapons: 12), 90, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 66,
            HullSlotType = HullSlotType.Weapon,
            Range = 1
        };
        public static readonly TechHullComponent Bludgeon = new TechHullComponent("Bludgeon", new Cost(0, 22, 0, 9), new TechRequirements(weapons: 13), 100, TechCategory.BeamWeapon)
        {
            Mass = 10,
            Initiative = 10,
            Power = 231,
            HullSlotType = HullSlotType.Weapon,
            Range = 0
        };
        public static readonly TechHullComponent MarkIVBlaster = new TechHullComponent("Mark IV Blaster", new Cost(0, 12, 0, 15), new TechRequirements(weapons: 14), 110, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 7,
            Power = 66,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        public static readonly TechHullComponent PhasedSapper = new TechHullComponent("Phased Sapper", new Cost(0, 0, 6, 16), new TechRequirements(energy: 8, weapons: 15), 120, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 14,
            DamageShieldsOnly = true,
            Power = 211,
            HullSlotType = HullSlotType.Weapon,
            Range = 3
        };
        public static readonly TechHullComponent HeavyBlaster = new TechHullComponent("Heavy Blaster", new Cost(0, 20, 0, 25), new TechRequirements(weapons: 16), 130, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 5,
            Power = 66,
            HullSlotType = HullSlotType.Weapon,
            Range = 3
        };
        public static readonly TechHullComponent GatlingNeutrinoCannon = new TechHullComponent("Gatling Neutrino Cannon", new Cost(0, 28, 0, 17), new TechRequirements(weapons: 17, prtRequired: PRT.WM), 140, TechCategory.BeamWeapon)
        {
            Mass = 3,
            Initiative = 13,
            MineSweep = 1280,
            Power = 80,
            HitsAllTargets = true,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        public static readonly TechHullComponent MyopicDisruptor = new TechHullComponent("Myopic Disruptor", new Cost(0, 14, 0, 12), new TechRequirements(weapons: 18), 150, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 169,
            HullSlotType = HullSlotType.Weapon,
            Range = 1
        };
        public static readonly TechHullComponent Blunderbuss = new TechHullComponent("Blunderbuss", new Cost(0, 30, 0, 13), new TechRequirements(weapons: 19, prtRequired: PRT.WM), 160, TechCategory.BeamWeapon)
        {
            Mass = 10,
            Initiative = 11,
            Power = 592,
            HullSlotType = HullSlotType.Weapon,
            Range = 0
        };
        public static readonly TechHullComponent Disruptor = new TechHullComponent("Disruptor", new Cost(0, 16, 0, 20), new TechRequirements(weapons: 20), 170, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 8,
            Power = 169,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        public static readonly TechHullComponent SyncroSapper = new TechHullComponent("Syncro Sapper", new Cost(0, 0, 8, 21), new TechRequirements(energy: 11, weapons: 21), 180, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 14,
            DamageShieldsOnly = true,
            Power = 541,
            HullSlotType = HullSlotType.Weapon,
            Range = 3
        };
        public static readonly TechHullComponent MegaDisruptor = new TechHullComponent("Mega Disruptor", new Cost(0, 30, 0, 33), new TechRequirements(weapons: 22), 190, TechCategory.BeamWeapon)
        {
            Mass = 2,
            Initiative = 6,
            Power = 169,
            HullSlotType = HullSlotType.Weapon,
            Range = 3
        };
        public static readonly TechHullComponent BigMuthaCannon = new TechHullComponent("Big Mutha Cannon", new Cost(0, 36, 0, 23), new TechRequirements(weapons: 23), 200, TechCategory.BeamWeapon)
        {
            Mass = 3,
            Initiative = 13,
            MineSweep = 3264,
            Power = 204,
            HitsAllTargets = true,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        public static readonly TechHullComponent StreamingPulverizer = new TechHullComponent("Streaming Pulverizer", new Cost(0, 20, 0, 16), new TechRequirements(weapons: 24), 210, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 9,
            Power = 433,
            HullSlotType = HullSlotType.Weapon,
            Range = 1
        };
        public static readonly TechHullComponent AntiMatterPulverizer = new TechHullComponent("Anti-Matter Pulverizer", new Cost(0, 22, 0, 27), new TechRequirements(weapons: 26), 220, TechCategory.BeamWeapon)
        {
            Mass = 1,
            Initiative = 8,
            Power = 433,
            HullSlotType = HullSlotType.Weapon,
            Range = 2
        };
        #endregion

        #region Torpedos
        public static readonly TechHullComponent AlphaTorpedo = new TechHullComponent("Alpha Torpedo", new Cost(8, 3, 3, 4), new TechRequirements(), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 0,
            Accuracy = 35,
            Power = 5,
            HullSlotType = HullSlotType.Weapon,
            Range = 4,
        };
        public static readonly TechHullComponent ArmageddonMissile = new TechHullComponent("Armageddon Missile", new Cost(67, 23, 16, 24), new TechRequirements(weapons: 24, propulsion: 10), 0, TechCategory.Torpedo)
        {
            Mass = 35,
            Initiative = 3,
            Accuracy = 30,
            CapitalShipMissile = true,
            Power = 525,
            HullSlotType = HullSlotType.Weapon,
            Range = 6,
        };
        public static readonly TechHullComponent BetaTorpedo = new TechHullComponent("Beta Torpedo", new Cost(18, 6, 4, 6), new TechRequirements(weapons: 5, propulsion: 1), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 1,
            Accuracy = 45,
            Power = 12,
            HullSlotType = HullSlotType.Weapon,
            Range = 4,
        };
        public static readonly TechHullComponent DeltaTorpedo = new TechHullComponent("Delta Torpedo", new Cost(22, 8, 5, 8), new TechRequirements(weapons: 10, propulsion: 2), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 1,
            Accuracy = 60,
            Power = 26,
            HullSlotType = HullSlotType.Weapon,
            Range = 4,
        };
        public static readonly TechHullComponent DoomsdayMissile = new TechHullComponent("Doomsday Missile", new Cost(60, 20, 13, 20), new TechRequirements(weapons: 20, propulsion: 10), 0, TechCategory.Torpedo)
        {
            Mass = 35,
            Initiative = 2,
            Accuracy = 25,
            CapitalShipMissile = true,
            Power = 280,
            HullSlotType = HullSlotType.Weapon,
            Range = 6,
        };
        public static readonly TechHullComponent EpsilonTorpedo = new TechHullComponent("Epsilon Torpedo", new Cost(30, 10, 6, 10), new TechRequirements(weapons: 14, propulsion: 3), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 2,
            Accuracy = 65,
            Power = 48,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        public static readonly TechHullComponent JihadMissile = new TechHullComponent("Jihad Missile", new Cost(37, 13, 9, 13), new TechRequirements(weapons: 12, propulsion: 6), 0, TechCategory.Torpedo)
        {
            Mass = 35,
            Accuracy = 20,
            CapitalShipMissile = true,
            Power = 85,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        public static readonly TechHullComponent JuggernautMissile = new TechHullComponent("Juggernaut Missile", new Cost(48, 16, 11, 16), new TechRequirements(weapons: 16, propulsion: 8), 0, TechCategory.Torpedo)
        {
            Mass = 35,
            Initiative = 1,
            Accuracy = 20,
            CapitalShipMissile = true,
            Power = 150,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        public static readonly TechHullComponent OmegaTorpedo = new TechHullComponent("Omega Torpedo", new Cost(52, 18, 12, 18), new TechRequirements(weapons: 26, propulsion: 6), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 4,
            Accuracy = 80,
            Power = 316,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        public static readonly TechHullComponent RhoTorpedo = new TechHullComponent("Rho Torpedo", new Cost(34, 12, 8, 12), new TechRequirements(weapons: 18, propulsion: 4), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 2,
            Accuracy = 75,
            Power = 90,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        public static readonly TechHullComponent UpsilonTorpedo = new TechHullComponent("Upsilon Torpedo", new Cost(40, 14, 9, 15), new TechRequirements(weapons: 22, propulsion: 5), 0, TechCategory.Torpedo)
        {
            Mass = 25,
            Initiative = 3,
            Accuracy = 75,
            Power = 169,
            HullSlotType = HullSlotType.Weapon,
            Range = 5,
        };
        #endregion

        #region Shields
        public static readonly TechHullComponent MoleSkinShield = new TechHullComponent("Mole-skin Shield", new Cost(1, 0, 1, 4), new TechRequirements(), 10, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 25,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent CowHideShield = new TechHullComponent("Cow-hide Shield", new Cost(2, 0, 2, 5), new TechRequirements(energy: 3), 20, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 40,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent WolverineDiffuseShield = new TechHullComponent("Wolverine Diffuse Shield", new Cost(3, 0, 3, 6), new TechRequirements(energy: 6), 30, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 60,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent CrobySharmor = new TechHullComponent("Croby Sharmor", new Cost(7, 0, 4, 15), new TechRequirements(energy: 7, construction: 4, prtRequired: PRT.IS), 40, TechCategory.Shield)
        {
            Mass = 10,
            Shield = 60,
            Armor = 65,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent ShadowShield = new TechHullComponent("Shadow Shield", new Cost(3, 0, 3, 7), new TechRequirements(energy: 7, electronics: 3, prtRequired: PRT.SS), 50, TechCategory.Shield)
        {
            Mass = 2,
            Shield = 75,
            Cloak = 35,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent BearNeutrinoBarrier = new TechHullComponent("Bear Neutrino Barrier", new Cost(4, 0, 4, 8), new TechRequirements(energy: 10), 60, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 100,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent GorillaDelagator = new TechHullComponent("Gorilla Delagator", new Cost(5, 0, 6, 11), new TechRequirements(energy: 14), 70, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 175,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent ElephantHideFortress = new TechHullComponent("Elephant Hide Fortress", new Cost(8, 0, 10, 15), new TechRequirements(energy: 18), 80, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 300,
            HullSlotType = HullSlotType.Shield,
        };
        public static readonly TechHullComponent CompletePhaseShield = new TechHullComponent("Complete Phase Shield", new Cost(12, 0, 15, 20), new TechRequirements(energy: 22), 90, TechCategory.Shield)
        {
            Mass = 1,
            Shield = 500,
            HullSlotType = HullSlotType.Shield,
        };
        #endregion

        #region ShipHulls

        public static readonly TechHull SmallFreighter = new TechHull("Small Freighter", new Cost(12, 0, 17, 20), new TechRequirements(), 10, TechCategory.ShipHull)
        {
            Type = TechHullType.Freighter,
            Mass = 25,
            Armor = 25,
            FuelCapacity = 130,
            CargoCapacity = 70,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 1, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 1, false)
            })
        };

        public static readonly TechHull MediumFreighter = new TechHull("Medium Freighter", new Cost(20, 0, 19, 40), new TechRequirements(construction: 3), 20, TechCategory.ShipHull)
        {
            Type = TechHullType.Freighter,
            Mass = 60,
            Armor = 50,
            FuelCapacity = 450,
            CargoCapacity = 210,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false)
            })
        };

        public static readonly TechHull LargeFreighter = new TechHull("Large Freighter", new Cost(35, 0, 21, 100), new TechRequirements(construction: 8), 30, TechCategory.ShipHull)
        {
            Type = TechHullType.Freighter,
            Mass = 125,
            Armor = 150,
            FuelCapacity = 2600,
            CargoCapacity = 1200,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 2, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false)
            })
        };

        public static readonly TechHull SuperFreighter = new TechHull("Super Freighter", new Cost(35, 0, 21, 100), new TechRequirements(construction: 13, prtRequired: PRT.IS), 40, TechCategory.ShipHull)
        {
            Type = TechHullType.Freighter,
            Mass = 175,
            Armor = 400,
            FuelCapacity = 8000,
            CargoCapacity = 3000,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 3, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 3, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 5, false),
                new TechHullSlot(HullSlotType.Electrical, 2, false)
            })
        };

        public static readonly TechHull Scout = new TechHull("Scout", new Cost(4, 2, 4, 9), new TechRequirements(), 50, TechCategory.ShipHull)
        {
            Type = TechHullType.Scout,
            Mass = 8,
            BuiltInScannerForJoaT = true,
            Armor = 20,
            Initiative = 1,
            FuelCapacity = 50,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Scanner, 1, false),
                new TechHullSlot(HullSlotType.General, 1, false)
            })
        };

        public static readonly TechHull Frigate = new TechHull("Frigate", new Cost(4, 2, 5, 12), new TechRequirements(construction: 6), 60, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 8,
            BuiltInScannerForJoaT = true,
            Armor = 45,
            Initiative = 4,
            FuelCapacity = 125,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Scanner, 1, false),
                new TechHullSlot(HullSlotType.General, 3, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false)
            })
        };


        public static readonly TechHull Destroyer = new TechHull("Destroyer", new Cost(15, 3, 5, 35), new TechRequirements(construction: 3), 70, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 30,
            BuiltInScannerForJoaT = true,
            Armor = 200,
            Initiative = 3,
            FuelCapacity = 280,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Weapon, 1, false),
                new TechHullSlot(HullSlotType.Weapon, 1, false),
                new TechHullSlot(HullSlotType.General, 1, false),
                new TechHullSlot(HullSlotType.Armor, 2, false),
                new TechHullSlot(HullSlotType.Mechanical, 1, false),
                new TechHullSlot(HullSlotType.Electrical, 1, false)
            })
        };

        public static readonly TechHull Cruiser = new TechHull("Cruiser", new Cost(40, 5, 8, 85), new TechRequirements(construction: 9), 80, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 90,
            Armor = 700,
            Initiative = 5,
            FuelCapacity = 600,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 2, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 1, false),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 1, false),
                new TechHullSlot(HullSlotType.Weapon, 2, false),
                new TechHullSlot(HullSlotType.Weapon, 2, false),
                new TechHullSlot(HullSlotType.General, 2, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false)
            })
        };

        public static readonly TechHull BattleCruiser = new TechHull("Battle Cruiser", new Cost(55, 8, 12, 120), new TechRequirements(construction: 9, prtRequired: PRT.WM), 90, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 120,
            Armor = 1000,
            Initiative = 5,
            FuelCapacity = 1400,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 2, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.Weapon, 3, false),
                new TechHullSlot(HullSlotType.Weapon, 3, false),
                new TechHullSlot(HullSlotType.General, 3, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 4, false)
            })
        };

        public static readonly TechHull Battleship = new TechHull("Battleship", new Cost(120, 25, 20, 225), new TechRequirements(construction: 13), 100, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 222,
            Armor = 2000,
            Initiative = 10,
            FuelCapacity = 2800,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 4, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical, 1, false),
                new TechHullSlot(HullSlotType.Shield, 8, false),
                new TechHullSlot(HullSlotType.Weapon, 6, false),
                new TechHullSlot(HullSlotType.Weapon, 6, false),
                new TechHullSlot(HullSlotType.Weapon, 2, false),
                new TechHullSlot(HullSlotType.Weapon, 2, false),
                new TechHullSlot(HullSlotType.Weapon, 4, false),
                new TechHullSlot(HullSlotType.Armor, 6, false),
                new TechHullSlot(HullSlotType.Electrical, 3, false),
                new TechHullSlot(HullSlotType.Electrical, 3, false),
            })
        };

        public static readonly TechHull Dreadnought = new TechHull("Dreadnought", new Cost(140, 30, 25, 275), new TechRequirements(construction: 16, prtRequired: PRT.WM), 110, TechCategory.ShipHull)
        {
            Type = TechHullType.Fighter,
            Mass = 250,
            Armor = 4500,
            Initiative = 10,
            FuelCapacity = 4500,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 5, true),
                new TechHullSlot(HullSlotType.ShieldArmor, 4, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 4, false),
                new TechHullSlot(HullSlotType.Weapon, 6, false),
                new TechHullSlot(HullSlotType.Weapon, 6, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Weapon, 8, false),
                new TechHullSlot(HullSlotType.Weapon, 8, false),
                new TechHullSlot(HullSlotType.Armor, 8, false),
                new TechHullSlot(HullSlotType.WeaponShield, 5, false),
                new TechHullSlot(HullSlotType.WeaponShield, 5, false),
                new TechHullSlot(HullSlotType.General, 2, false),
            })
        };

        public static readonly TechHull Privateer = new TechHull("Privateer", new Cost(50, 3, 3, 50), new TechRequirements(construction: 4), 120, TechCategory.ShipHull)
        {
            Type = TechHullType.ArmedFreighter,
            Mass = 65,
            Armor = 150,
            Initiative = 3,
            FuelCapacity = 650,
            CargoCapacity = 250,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false),
                new TechHullSlot(HullSlotType.ShieldElectricalMechanical, 1, false),
                new TechHullSlot(HullSlotType.General, 1, false),
                new TechHullSlot(HullSlotType.General, 1, false)
            })
        };

        public static readonly TechHull Rogue = new TechHull("Rogue", new Cost(80, 5, 5, 60), new TechRequirements(construction: 8, prtRequired: PRT.SS), 130, TechCategory.ShipHull)
        {
            Type = TechHullType.ArmedFreighter,
            Mass = 75,
            Armor = 450,
            Initiative = 4,
            FuelCapacity = 2250,
            CargoCapacity = 500,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 2, true),
                new TechHullSlot(HullSlotType.ShieldArmor, 3, false),
                new TechHullSlot(HullSlotType.MineElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.Scanner, 1, false),
                new TechHullSlot(HullSlotType.General, 2, false),
                new TechHullSlot(HullSlotType.General, 2, false),
                new TechHullSlot(HullSlotType.MineElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.Electrical, 1, false),
                new TechHullSlot(HullSlotType.Electrical, 1, false),
            })
        };

        public static readonly TechHull Galleon = new TechHull("Galleon", new Cost(70, 5, 5, 105), new TechRequirements(construction: 11), 140, TechCategory.ShipHull)
        {
            Type = TechHullType.ArmedFreighter,
            Mass = 125,
            Armor = 900,
            Initiative = 4,
            FuelCapacity = 2500,
            CargoCapacity = 1000,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 4, true),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 2, false),
                new TechHullSlot(HullSlotType.General, 3, false),
                new TechHullSlot(HullSlotType.General, 3, false),
                new TechHullSlot(HullSlotType.MineElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.MineElectricalMechanical, 2, false),
                new TechHullSlot(HullSlotType.Scanner, 2, false),
            })
        };

        public static readonly TechHull MiniColonyShip = new TechHull("Mini-Colony Ship", new Cost(2, 0, 2, 3), new TechRequirements(prtRequired: PRT.HE), 150, TechCategory.ShipHull)
        {
            Type = TechHullType.Colonizer,
            Mass = 8,
            Armor = 10,
            FuelCapacity = 150,
            CargoCapacity = 10,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Mechanical, 1, false)
            })
        };

        public static readonly TechHull ColonyShip = new TechHull("Colony Ship", new Cost(9, 0, 13, 18), new TechRequirements(), 160, TechCategory.ShipHull)
        {
            Type = TechHullType.Colonizer,
            Mass = 20,
            Armor = 20,
            FuelCapacity = 200,
            CargoCapacity = 25,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Mechanical, 1, false)
            })
        };

        public static readonly TechHull MiniBomber = new TechHull("Mini Bomber", new Cost(18, 5, 9, 32), new TechRequirements(construction: 1), 170, TechCategory.ShipHull)
        {
            Type = TechHullType.Bomber,
            Mass = 28,
            Armor = 50,
            FuelCapacity = 120,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Bomb, 2, false),
            })
        };

        public static readonly TechHull MiniMiner = new TechHull("Mini-Miner", new Cost(25, 0, 6, 50), new TechRequirements(construction: 2), 220, TechCategory.ShipHull)
        {
            Type = TechHullType.Miner,
            Mass = 80,
            Armor = 130,
            FuelCapacity = 210,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.ScannerElectricalMechanical),
                new TechHullSlot(HullSlotType.Mining),
                new TechHullSlot(HullSlotType.Mining)
            })
        };

        public static readonly TechHull FuelTransport = new TechHull("Fuel Transport", new Cost(10, 0, 5, 50), new TechRequirements(construction: 4, prtRequired: PRT.IS), 260, TechCategory.ShipHull)
        {
            Type = TechHullType.FuelTransport,
            Mass = 12,
            Armor = 5,
            FuelCapacity = 750,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Shield),
            })
        };

        #endregion

        #region StarbaseHulls
        public static readonly TechHull SpaceStation = new TechHull("Space Station", new Cost(106, 71, 220, 528), new TechRequirements(), 20, TechCategory.StarbaseHull)
        {
            Type = TechHullType.Starbase,
            Mass = 0,
            Armor = 500,
            Initiative = 14,
            RangeBonus = 1,
            Starbase = true,
            SpaceDock = TechHull.UnlimitedSpaceDock,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.OrbitalElectrical, 1, false),
                new TechHullSlot(HullSlotType.Weapon, 16, false),
                new TechHullSlot(HullSlotType.Shield, 16, false),
                new TechHullSlot(HullSlotType.Weapon, 16, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 16, false),
                new TechHullSlot(HullSlotType.Shield, 16, false),
                new TechHullSlot(HullSlotType.Electrical, 3, false),
                new TechHullSlot(HullSlotType.Weapon, 16, false),
                new TechHullSlot(HullSlotType.Electrical, 3, false),
                new TechHullSlot(HullSlotType.Weapon, 16, false),
                new TechHullSlot(HullSlotType.OrbitalElectrical, 1, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 16, false),
            })
        };

        public static readonly TechHull DeathStar = new TechHull("Death Star", new Cost(120, 80, 350, 750), new TechRequirements(construction: 17, prtRequired: PRT.AR), 40, TechCategory.StarbaseHull)
        {
            Type = TechHullType.Starbase,
            Mass = 0,
            Armor = 1500,
            Initiative = 18,
            Starbase = true,
            SpaceDock = TechHull.UnlimitedSpaceDock,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.OrbitalElectrical, 1, false),
                new TechHullSlot(HullSlotType.Weapon, 32, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Shield, 20, false),
                new TechHullSlot(HullSlotType.Shield, 20, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Weapon, 32, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Weapon, 32, false),
                new TechHullSlot(HullSlotType.OrbitalElectrical, 1, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 20, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.ShieldArmor, 20, false),
                new TechHullSlot(HullSlotType.Electrical, 4, false),
                new TechHullSlot(HullSlotType.Weapon, 32, false),
            })
        };
        #endregion

        #region Defenses

        public static readonly TechDefense SDI = new TechDefense("SDI", new Cost(5, 5, 5, 15), new TechRequirements(prtDenied: PRT.AR), 0, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = .99f
        };
        public static readonly TechDefense MissileBattery = new TechDefense("Missile Battery", new Cost(5, 5, 5, 15), new TechRequirements(energy: 5, prtDenied: PRT.AR), 10, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 1.99f
        };
        public static readonly TechDefense LaserBattery = new TechDefense("Laser Battery", new Cost(5, 5, 5, 15), new TechRequirements(energy: 10, prtDenied: PRT.AR), 10, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 2.39f
        };
        public static readonly TechDefense PlanetaryShield = new TechDefense("Planetary Shield", new Cost(5, 5, 5, 15), new TechRequirements(energy: 16, prtDenied: PRT.AR), 10, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 2.99f
        };
        public static readonly TechDefense NeutronShield = new TechDefense("Neutron Shield", new Cost(5, 5, 5, 15), new TechRequirements(energy: 23, prtDenied: PRT.AR), 10, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 3.79f
        };

        #endregion

        #region Planetary Scanners

        public static readonly TechPlanetaryScanner Viewer50 = new TechPlanetaryScanner("Viewer 50", new Cost(10, 10, 70, 100), new TechRequirements(prtDenied: PRT.AR), 0, TechCategory.PlanetaryScanner)
        {
            ScanRange = 50,
            ScanRangePen = 0
        };

        public static readonly TechPlanetaryScanner Viewer90 = new TechPlanetaryScanner("Viewer 90", new Cost(10, 10, 70, 100), new TechRequirements(electronics: 1, prtDenied: PRT.AR), 1, TechCategory.PlanetaryScanner)
        {
            ScanRange = 90,
            ScanRangePen = 0
        };

        public static readonly TechPlanetaryScanner Scoper150 = new TechPlanetaryScanner("Scoper 150", new Cost(10, 10, 70, 100), new TechRequirements(electronics: 3, prtDenied: PRT.AR), 30, TechCategory.PlanetaryScanner)
        {
            ScanRange = 150,
            ScanRangePen = 0,
        };

        public static readonly TechPlanetaryScanner Scoper220 = new TechPlanetaryScanner("Scoper 220", new Cost(10, 10, 70, 100), new TechRequirements(electronics: 6, prtDenied: PRT.AR), 40, TechCategory.PlanetaryScanner)
        {
            ScanRange = 220,
            ScanRangePen = 0,
        };

        public static readonly TechPlanetaryScanner Scoper280 = new TechPlanetaryScanner("Scoper 280", new Cost(10, 10, 70, 100), new TechRequirements(electronics: 8, prtDenied: PRT.AR), 50, TechCategory.PlanetaryScanner)
        {
            ScanRange = 280,
            ScanRangePen = 0,
        };

        public static readonly TechPlanetaryScanner Snooper320X = new TechPlanetaryScanner("Snooper 320X", new Cost(10, 10, 70, 100), new TechRequirements(energy: 3, electronics: 10, biotechnology: 3, prtDenied: PRT.AR, lrtsDenied: LRT.NAS), 60, TechCategory.PlanetaryScanner)
        {
            ScanRange = 320,
            ScanRangePen = 160,
        };

        public static readonly TechPlanetaryScanner Snooper400X = new TechPlanetaryScanner("Snooper 400X", new Cost(10, 10, 70, 100), new TechRequirements(energy: 4, electronics: 13, biotechnology: 6, prtDenied: PRT.AR, lrtsDenied: LRT.NAS), 70, TechCategory.PlanetaryScanner)
        {
            ScanRange = 400,
            ScanRangePen = 200,
        };

        public static readonly TechPlanetaryScanner Snooper500X = new TechPlanetaryScanner("Snooper 500X", new Cost(10, 10, 70, 100), new TechRequirements(energy: 5, electronics: 16, biotechnology: 7, prtDenied: PRT.AR, lrtsDenied: LRT.NAS), 80, TechCategory.PlanetaryScanner)
        {
            ScanRange = 500,
            ScanRangePen = 250,
        };

        public static readonly TechPlanetaryScanner Snooper620X = new TechPlanetaryScanner("Snooper 620X", new Cost(10, 10, 70, 100), new TechRequirements(energy: 7, electronics: 23, biotechnology: 9, prtDenied: PRT.AR, lrtsDenied: LRT.NAS), 90, TechCategory.PlanetaryScanner)
        {
            ScanRange = 620,
            ScanRangePen = 310,
        };

        #endregion

        static Techs()
        {
            AllTechs.AddRange(new Tech[] {
                // engines
                SettlersDelight,
                QuickJump5,
                LongHump6,
                FuelMizer,
                RadiatingHydroRamScoop,
                DaddyLongLegs7,
                AlphaDrive8,
                TransGalacticDrive,
                TransGalacticFuelScoop,
                TransGalacticMizerScoop,
                TransGalacticSuperScoop,
                GalaxyScoop,
                Interspace10,
                TransStar10,
                SubGalacticFuelScoop,

                // mass drivers
                MassDriver5,
                MassDriver6,
                MassDriver7,
                SuperDriver8,
                SuperDriver9,
                UltraDriver10,
                UltraDriver11,
                UltraDriver12,
                UltraDriver13,

                // stargates
                Stargate100_250,
                Stargate100_Any,
                Stargate150_600,
                Stargate300_500,
                StargateAny_300,
                StargateAny_800,
                StargateAny_Any,

                // miners
                OrbitalAdjuster,
                RoboMiner,
                RoboMaxiMiner,
                RoboMidgetMiner,
                RoboMiniMiner,
                RoboSuperMiner,
                RoboUltraMiner, 

                // bombs
                LadyFingerBomb,
                BlackCatBomb,
                M70Bomb,
                M80Bomb,
                CherryBomb,
                LBU17Bomb,
                LBU32Bomb,
                LBU74Bomb,
                RetroBomb,
                SmartBomb,
                NeutronBomb,
                EnrichedNeutronBomb,
                PeerlessBomb,
                AnnihilatorBomb, 

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
                RobberBaronScanner,

                // Armor
                Tritanium,
                Crobmnium,
                Carbonic,
                Strobnium,
                Organic,
                Kelarium,
                Fielded,
                DepletedNeutronium,
                Neutronium,
                Valanium,
                Superlatanium,

                // Electronics
                BattleComputer,
                BattleNexus,
                BattleSuperComputer,
                EnergyCapacitor,
                EnergyDampener,
                FluxCapacitor,
                Jammer10,
                Jammer20,
                Jammer30,
                Jammer50,

                // Cloaks
                StealthCloak,
                SuperStealthCloak,
                TachyonDetector,
                TransportCloaking,
                UltraStealthCloak,

                // Mine Layers
                HeavyDispenser110,
                HeavyDispenser200,
                HeavyDispenser50,
                MineDispenser130,
                MineDispenser40,
                MineDispenser50,
                MineDispenser80,
                SpeedTrap20,
                SpeedTrap30,
                SpeedTrap50,

                // mechanical
                BeamDeflector,
                CargoPod,
                ColonizationModule,
                FuelTank,
                ManeuveringJet,
                OrbitalConstructionModule,
                Overthruster,
                SuperCargoPod,
                SuperFuelTank,

                // BeamWeapons
                Laser,
                XRayLaser,
                MiniGun,
                YakimoraLightPhaser,
                Blackjack,
                PhaserBazooka,
                PulsedSapper,
                ColloidalPhaser,
                GatlingGun,
                MiniBlaster,
                Bludgeon,
                MarkIVBlaster,
                PhasedSapper,
                HeavyBlaster,
                GatlingNeutrinoCannon,
                MyopicDisruptor,
                Blunderbuss,
                Disruptor,
                SyncroSapper,
                MegaDisruptor,
                BigMuthaCannon,
                StreamingPulverizer,
                AntiMatterPulverizer,
                
                // Mystery Trader
                AntiMatterGenerator,
                
                // Shields
                MoleSkinShield,
                CowHideShield,
                WolverineDiffuseShield,
                CrobySharmor,
                ShadowShield,
                BearNeutrinoBarrier,
                GorillaDelagator,
                ElephantHideFortress,
                CompletePhaseShield,

                // Torpedos
                AlphaTorpedo,
                ArmageddonMissile,
                BetaTorpedo,
                DeltaTorpedo,
                DoomsdayMissile,
                EpsilonTorpedo,
                JihadMissile,
                JuggernautMissile,
                OmegaTorpedo,
                RhoTorpedo,
                UpsilonTorpedo,

                // ship hulls,
                SmallFreighter,
                MediumFreighter,
                LargeFreighter,
                SuperFreighter,
                Scout,
                Frigate,
                Destroyer,
                Cruiser,
                BattleCruiser,
                Battleship,
                Dreadnought,
                Privateer,
                Rogue,
                Galleon,
                MiniColonyShip,
                ColonyShip,
                MiniBomber,
                MiniMiner,
                FuelTransport,

                // starbases
                SpaceStation,
                DeathStar,

                // defenses
                MissileBattery,
                SDI,
                LaserBattery,
                PlanetaryShield,
                NeutronShield,

                // Planetary Scanner
                Viewer50,
                Viewer90,
                Scoper150,
                Scoper220,
                Scoper280,
                Snooper320X,
                Snooper400X,
                Snooper500X,
                Snooper620X,
            });
        }
    }
}