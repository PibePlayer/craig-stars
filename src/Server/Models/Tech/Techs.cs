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

        public static readonly TechEngine LongHump6 = new TechEngine("Long Hump 6", new Cost(5, 0, 1, 6), new TechRequirements(propulsion: 3), 30)
        {
            Mass = 9,
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

        #region Mechanical
        public static readonly TechHullComponent BeamDeflector = new TechHullComponent("Beam Deflector", new Cost(0, 0, 10, 8), new TechRequirements(energy: 6, weapons: 6, construction: 6, electronics: 6), 0, TechCategory.Mechanical)
        {
            Mass = 1,
            HullSlotType = HullSlotType.Mechanical,
            BeamDefense = 1
        };
        public static readonly TechHullComponent CargoPod = new TechHullComponent("Cargo Pod", new Cost(5, 0, 2, 10), new TechRequirements(construction: 3), 0, TechCategory.Mechanical)
        {
            Mass = 5,
            CargoBonus = 50,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent ColonizationModule = new TechHullComponent("Colonization Module", new Cost(11, 9, 9, 9), new TechRequirements(), 0, TechCategory.Mechanical)
        {
            Mass = 32,
            ColonizationModule = true,
            HullSlotType = HullSlotType.Mechanical
        };
        public static readonly TechHullComponent FuelTank = new TechHullComponent("Fuel Tank", new Cost(5, 0, 0, 4), new TechRequirements(), 0, TechCategory.Mechanical)
        {
            Mass = 3,
            FuelBonus = 250,
            HullSlotType = HullSlotType.Mechanical
        };
        public static readonly TechHullComponent ManeuveringJet = new TechHullComponent("Maneuvering Jet", new Cost(5, 0, 5, 10), new TechRequirements(energy: 2, propulsion: 3), 0, TechCategory.Mechanical)
        {
            Mass = 5,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent OrbitalConstructionModule = new TechHullComponent("Orbital Construction Module", new Cost(18, 13, 13, 18), new TechRequirements(prtRequired: PRT.AR), 0, TechCategory.Mechanical)
        {
            Mass = 50,
            MinKillRate = 2000,
            ColonizationModule = true,
            HullSlotType = HullSlotType.Armor
        };
        public static readonly TechHullComponent Overthruster = new TechHullComponent("Overthruster", new Cost(10, 0, 8, 20), new TechRequirements(energy: 5, propulsion: 12), 0, TechCategory.Mechanical)
        {
            Mass = 5,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent SuperCargoPod = new TechHullComponent("Super Cargo Pod", new Cost(8, 0, 2, 15), new TechRequirements(energy: 3, construction: 8), 0, TechCategory.Mechanical)
        {
            Mass = 7,
            CargoBonus = 100,
            HullSlotType = HullSlotType.Mechanical,
        };
        public static readonly TechHullComponent SuperFuelTank = new TechHullComponent("Super Fuel Tank", new Cost(8, 0, 0, 8), new TechRequirements(energy: 6, propulsion: 4, construction: 14), 0, TechCategory.Mechanical)
        {
            Mass = 8,
            FuelBonus = 500,
            HullSlotType = HullSlotType.Mechanical,
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
        public static readonly TechHullComponent MiniGun = new TechHullComponent("Mini Gun", new Cost(0, 6, 0, 6), new TechRequirements(weapons: 5), 20, TechCategory.BeamWeapon)
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

        #region MysteryTrader
        public static readonly TechHullComponent AntiMatterGenerator = new TechHullComponent("Anti-Matter Generator", new Cost(8, 3, 3, 10), new TechRequirements(weapons: 12, biotechnology: 7, prtRequired: PRT.IT), 0, TechCategory.Electrical)
        {
            Mass = 10,
            FuelRegenerationRate = 50,
            FuelBonus = 200,
            HullSlotType = HullSlotType.Electrical,
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

        public static readonly TechHull Scout = new TechHull("Scout", new Cost(4, 2, 4, 9), new TechRequirements(), 50, TechCategory.ShipHull)
        {
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

        public static readonly TechHull ColonyShip = new TechHull("Colony Ship", new Cost(9, 0, 13, 18), new TechRequirements(), 160, TechCategory.ShipHull)
        {
            Mass = 20,
            Armor = 20,
            FuelCapacity = 200,
            CargoCapacity = 25,
            Slots = new List<TechHullSlot>(new TechHullSlot[] {
                new TechHullSlot(HullSlotType.Engine, 1, true),
                new TechHullSlot(HullSlotType.Mechanical, 1, false)
            })
        };

        #endregion

        #region StarbaseHulls
        public static readonly TechHull SpaceStation = new TechHull("Space Station", new Cost(106, 71, 220, 528), new TechRequirements(), 20, TechCategory.StarbaseHull)
        {
            Mass = 0,
            Armor = 500,
            Initiative = 14,
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

        #endregion

        #region Defenses

        public static readonly TechDefense MissileBattery = new TechDefense("Missile Battery", new Cost(5, 5, 5, 15), new TechRequirements(), 0, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 199
        };
        public static readonly TechDefense SDI = new TechDefense("SDI", new Cost(5, 5, 5, 15), new TechRequirements(), 100, TechCategory.PlanetaryDefense)
        {
            DefenseCoverage = 99
        };

        #endregion

        #region Planetary Scanners

        public static readonly TechPlanetaryScanner Viewer50 = new TechPlanetaryScanner("Viewer 50", new Cost(10, 10, 70, 100), new TechRequirements(), 0, TechCategory.PlanetaryScanner)
        {
            ScanRange = 50,
            ScanRangePen = 0
        };

        public static readonly TechPlanetaryScanner Viewer90 = new TechPlanetaryScanner("Viewer 90", new Cost(10, 10, 70, 100), new TechRequirements(electronics: 1), 1, TechCategory.PlanetaryScanner)
        {
            ScanRange = 90,
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
                Scout,
                ColonyShip,

                // starbases
                SpaceStation,

                // defenses
                MissileBattery,
                SDI,

                // Planetary Scanner
                Viewer50,
                Viewer90

            });
        }
    }
}