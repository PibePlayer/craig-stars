using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CraigStars
{
    /// <summary>
    /// The specifications for built in PRTs
    /// </summary>
    public class PRTSpecs
    {
        public List<PRTSpec> Specs { get; set; } = new List<PRTSpec>()
        {
            HE,
            SS,
            WM,
            CA,
            IS,
            SD,
            PP,
            IT,
            AR,
            JoaT
        };

        /// <summary>
        /// Get a PRTSpec by prt (this is just an index lookup)
        /// </summary>
        /// <returns></returns>
        public PRTSpec this[PRT prt] => Specs[(int)prt];

        public static PRTSpec HE = new PRTSpec(PRT.HE, -40)
        {
            StartingFleets = new()
            {
                new StartingFleet("Deep Space Probe", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Spore Cloud", Techs.MiniColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Spore Cloud", Techs.MiniColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Spore Cloud", Techs.MiniColonyShip.Name, ShipDesignPurpose.Colonizer)
            },
            GrowthFactor = 2f,
            MaxPopulationOffset = -.5f,
        };

        public static PRTSpec SS = new PRTSpec(PRT.SS, -95)
        {
            StartingTechLevels = new TechLevel(electronics: 5),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
            },
            BuiltInCloakUnits = 300,
            FreeCargoCloaking = true,
            MineFieldSafeWarpBonus = 1,
            StealsResearch = new StealsResearch(.5f, .5f, .5f, .5f, .5f, .5f),
        };

        public static PRTSpec WM = new PRTSpec(PRT.WM, -45)
        {
            StartingTechLevels = new TechLevel(energy: 1, weapons: 6, propulsion: 1),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Armed Probe", Techs.Scout.Name, ShipDesignPurpose.FighterScout),
            },
            TechCostFactor = new Dictionary<TechCategory, float>() {
                { TechCategory.BeamWeapon, .75f }, // weapons cost 25% less
                { TechCategory.Torpedo, .75f }, // weapons cost 25% less
                { TechCategory.Bomb, .75f }, // weapons cost 25% less
            },
            DiscoverDesignOnScan = true,
            InvasionAttackBonus = 1.65f,
            MovementBonus = 2,
        };

        public static PRTSpec CA = new PRTSpec(PRT.CA, -10)
        {
            StartingTechLevels = new TechLevel(energy: 1, weapons: 1, propulsion: 1, construction: 2, biotechnology: 6),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Change of Heart", Techs.MiniMiner.Name, ShipDesignPurpose.Terraformer),
            },
            Instaforming = true,
            PermaformChance = .1f, // chance is 10% if pop is over 100k
            PermaformPopulation = 100_000,
        };

        public static PRTSpec IS = new PRTSpec(PRT.IS, 100)
        {
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
            },
            TechCostFactor = new Dictionary<TechCategory, float>() {
                { TechCategory.PlanetaryDefense, .6f }, // defenses cost 40% less
                { TechCategory.BeamWeapon, 1.25f }, // weapons cost 25% less
                { TechCategory.Torpedo, 1.25f }, // weapons cost 25% less
                { TechCategory.Bomb, 1.25f }, // weapons cost 25% less
            },
            FreighterGrowthFactor = .5f,
            InvasionDefendBonus = 2f,
            RepairFactor = 2f, // double repairs!
            StarbaseRepairFactor = 1.5f,
        };

        public static PRTSpec SD = new PRTSpec(PRT.SD, 150)
        {
            StartingTechLevels = new TechLevel(propulsion: 2, biotechnology: 2),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Little Hen", Techs.MiniMineLayer.Name, ShipDesignPurpose.DamageMineLayer),
                new StartingFleet("Speed Turtle", Techs.MiniMineLayer.Name, ShipDesignPurpose.SpeedMineLayer),
            },
            MineFieldsAreScanners = true,
            CanDetonateMineFields = true,
            MineFieldMinDecayFactor = .25f,
            MineFieldSafeWarpBonus = 2,
        };

        public static PRTSpec PP = new PRTSpec(PRT.PP, -120)
        {
            StartingTechLevels = new TechLevel(energy: 4),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
            },
            StartingPlanets = new List<StartingPlanet>()
            {
                // one homeworld, 20k people, no hab penalty
                new StartingPlanet(25000, 0, HasMassDriver: true),
                // on extra world where hab varies by 1/2 of the range
                new StartingPlanet(10000, 1, HasMassDriver: true, StartingFleets: new List<StartingFleet>() {
                    new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                })
            },
            MineralsPerSingleMineralPacket = 70,
            MineralsPerMixedMineralPacket = 25,
            PacketResourceCost = 5,
            PacketMineralCostFactor = 1,
            PacketDecayFactor = .5f,
            PacketBuiltInScanner = true,
            DetectPacketDestinationStarbases = true,
            DetectAllPackets = true,
            PacketTerraformChance = .5f, // 50% per 100kT uncaught
            PacketPermaformChance = .001f, // .1% per 100kT uncaught

        };

        public static PRTSpec IT = new PRTSpec(PRT.IT, -180)
        {
            StartingTechLevels = new TechLevel(propulsion: 5, construction: 5),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Swashbuckler", Techs.Privateer.Name, ShipDesignPurpose.ArmedFreighter),
                new StartingFleet("Stalwart Defender", Techs.Destroyer.Name, ShipDesignPurpose.FighterScout),
            },
            StartingPlanets = new List<StartingPlanet>()
            {
                // one homeworld, 20k people, no hab penalty
                new StartingPlanet(25000, 0, HasStargate: true),
                // on extra world where hab varies by 1/2 of the range
                new StartingPlanet(10000, 1, HasStargate: true, StartingFleets: new List<StartingFleet>() {
                    new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                })
            },
            CanGateCargo = true,
            ShipsVanishInVoid = false,
            PacketMineralCostFactor = 1.2f,
            PacketReceiverFactor = .5f,
            PacketOverSafeWarpPenalty = 1,
        };

        public static PRTSpec AR = new PRTSpec(PRT.AR, -90)
        {
            StartingTechLevels = new TechLevel(energy: 1),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
            },
            CanRemoteMineOwnPlanets = true,
            StarbaseCostFactor = .8f,
        };

        public static PRTSpec JoaT = new PRTSpec(PRT.JoaT, 66)
        {
            StartingTechLevels = new TechLevel(energy: 3, weapons: 3, propulsion: 3, construction: 3, electronics: 3, biotechnology: 3),
            StartingFleets = new()
            {
                new StartingFleet("Long Range Scout", Techs.Scout.Name, ShipDesignPurpose.Scout),
                new StartingFleet("Santa Maria", Techs.ColonyShip.Name, ShipDesignPurpose.Colonizer),
                new StartingFleet("Teamster", Techs.MediumFreighter.Name, ShipDesignPurpose.Freighter),
                new StartingFleet("Cotton Picker", Techs.MiniMiner.Name, ShipDesignPurpose.Miner),
                new StartingFleet("Armed Probe", Techs.Scout.Name, ShipDesignPurpose.FighterScout),
                new StartingFleet("Stalwart Defender", Techs.Destroyer.Name, ShipDesignPurpose.FighterScout),
            },
            MaxPopulationOffset = .2f,
            BuiltInScannerMultiplier = 20,
            TechsCostExtraLevel = 4,
        };
    }
}