using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The specification for a race, based on it's PRT and LRTs
    /// </summary>
    public class RaceSpec : IPRTSpec, ILRTSpec
    {

        public TechLevel StartingTechLevels { get; set; } = new();

        /// <summary>
        /// Starting fleets this trait includes
        /// </summary>
        public List<StartingFleet> StartingFleets { get; set; } = new();

        /// <summary>
        /// Spec of starting planets this trait includes
        /// </summary>
        public List<StartingPlanet> StartingPlanets { get; set; } = new List<StartingPlanet>()
        {
            // default with one planet, 25k people, no hab penalty
            new StartingPlanet(25000)
        };

        /// <summary>
        /// The cost factor this rate applies to various TechCategories
        /// </summary>
        /// <returns></returns>
        public Dictionary<TechCategory, float> TechCostFactor { get; set; } = new();

        /// <summary>
        /// The race computed costs of various item types like mines, packets, etc
        /// </summary>
        /// <returns></returns>
        public Dictionary<QueueItemType, Cost> Costs = new();

        /// <summary>
        /// The amount of minerals in a single ironium, boranium, or germanium packet
        /// </summary>
        /// <value></value>
        public int MineralsPerSingleMineralPacket { get; set; } = 100;

        /// <summary>
        /// The amount of minerals in a mixed (all minerals at once) packet
        /// </summary>
        /// <value></value>
        public int MineralsPerMixedMineralPacket { get; set; } = 40;

        /// <summary>
        /// The cost, in resources, to create packets
        /// </summary>
        public int PacketResourceCost { get; set; } = 10;

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        public float PacketMineralCostFactor { get; set; } = 1.1f;

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        public float PacketReceiverFactor { get; set; } = 1f;

        /// <summary>
        /// Factor impacting the decay rate for packets. Some races packets decay faster
        /// </summary>
        public float PacketDecayFactor { get; set; } = 1f;

        /// <summary>
        /// Some races are so bad at flinging packets, their safe warp is always worse
        /// </summary>
        /// <value></value>
        public int PacketOverSafeWarpPenalty { get; set; } = 0;

        /// <summary>
        /// Do packets have a built in scanner?
        /// </summary>
        public bool PacketBuiltInScanner { get; set; }

        /// <summary>
        /// Do packets detect the destination starbase when they arrive?
        /// </summary>
        public bool DetectPacketDestinationStarbases { get; set; }

        /// <summary>
        /// Does this race detect all packets in space?
        /// </summary>
        /// <value></value>
        public bool DetectAllPackets { get; set; }

        /// <summary>
        /// Chance this race's packets terraform the receiving planet in this race's favor
        /// </summary>
        /// <value></value>
        public float PacketTerraformChance { get; set; }

        /// <summary>
        /// Chance that this race's packets permaform the receiving planet in this race's favor
        /// This only applies to every PacketPermaformSizeUnit kT of uncaught resources
        /// </summary>
        /// <value></value>
        public float PacketPermaformChance { get; set; }

        /// <summary>
        /// The unit of uncaught packet size for permaforming, i.e. every 100kT of uncaught minerals
        /// has a .1% chance of permaforming the planet
        /// </summary>
        /// <value></value>
        public int PacketPermaTerraformSizeUnit { get; set; }

        /// <summary>
        /// Can this rate send cargo through stargates?
        /// </summary>
        public bool CanGateCargo { get; set; } = false;

        /// <summary>
        /// Does this race lose ships to the endless void if they overgate?
        /// </summary>
        public bool ShipsVanishInVoid { get; set; } = true;

        /// <summary>
        /// Some races can operate the built in scanners on certain hulls
        /// </summary>
        public int BuiltInScannerMultiplier { get; set; } = 0;

        /// <summary>
        /// The level all techs that "cost extra" to research start at
        /// </summary>
        public int TechsCostExtraLevel { get; set; } = 3;

        /// <summary>
        /// The rate of growth for colonists on freighters
        /// </summary>
        public float FreighterGrowthFactor { get; set; } = 0f;

        /// <summary>
        /// Multiplier impacting base growth rate (i.e. HE races are 2x)
        /// </summary>
        public float GrowthFactor { get; set; } = 1f;

        /// <summary>
        /// Bonus to max pop, in percent. I.e. .1f would give a 10% bonus to max pop. this field stacks
        /// with LRTs
        /// </summary>
        public float MaxPopulationOffset { get; set; } = 0f;

        /// <summary>
        /// SS races have 300 cloak units built in (or 75% cloaking)
        /// </summary>
        public int BuiltInCloakUnits { get; set; } = 0;

        /// <summary>
        /// SS races steal research
        /// </summary>
        public StealsResearch StealsResearch { get; set; } = new StealsResearch();

        /// <summary>
        /// Does this race cloak cargo for free? (i.e. cargo doesn't count towards mass for cloak calcs)
        /// </summary>
        public bool FreeCargoCloaking { get; set; } = false;

        /// <summary>
        /// Do minefields act as scanners?
        /// </summary>
        public bool MineFieldsAreScanners { get; set; } = false;

        /// <summary>
        /// Space Demolition fleets can travel 2 warp speeds faster through minefields
        /// </summary>
        public int MineFieldSafeWarpBonus { get; set; } = 0;

        /// <summary>
        /// The minimum rate minefields decay
        /// </summary>
        public float MineFieldMinDecayFactor { get; set; } = 1f;

        /// <summary>
        /// The base rate minefields decay
        /// </summary>
        public float MineFieldBaseDecayRate { get; set; } = .02f;

        /// <summary>
        /// The rate minefields decay per planet they surround
        /// </summary>
        public float MineFieldPlanetDecayRate { get; set; } = .04f;

        /// <summary>
        /// The max rate minefields will decay
        /// </summary>
        public float MineFieldMaxDecayRate { get; set; } = .5f;

        /// <summary>
        /// Can this race detonate minefields
        /// </summary>
        public bool CanDetonateMineFields { get; set; } = false;

        /// <summary>
        /// The number of mines that are lost to detonation each turn
        /// </summary>
        public float MineFieldDetonateDecayRate { get; set; } = .25f;

        /// <summary>
        /// Does this race discover details about a fleet design when scanning? (this is normally only discovered in battle)
        /// </summary>
        public bool DiscoverDesignOnScan { get; set; } = false;

        /// <summary>
        /// Can this race remote mine their own planets?
        /// </summary>
        public bool CanRemoteMineOwnPlanets { get; set; } = false;

        /// <summary>
        /// The factor that invaders get when invading planets
        /// </summary>
        public float InvasionAttackBonus { get; set; } = 1.1f;

        /// <summary>
        /// The factor defenders get when defending from planet invasion
        /// </summary>
        public float InvasionDefendBonus { get; set; } = 1f;

        /// <summary>
        /// Increase in battle movement for this race
        /// </summary>
        public int MovementBonus { get; set; } = 0;

        /// <summary>
        /// Does the race instaform new worlds
        /// </summary>
        /// <value></value>
        public bool Instaforming { get; set; } = false;

        /// <summary>
        /// The base chance for permaforming a world (increases with pop)
        /// </summary>
        /// <value></value>
        public float PermaformChance { get; set; } = 0f;

        /// <summary>
        /// The maximum permaform chance this race has
        /// </summary>
        /// <value></value>
        public float MaxPermaformChance { get; set; } = 0f;

        /// <summary>
        /// The Population where a race's permaform chances top out
        /// </summary>
        /// <value></value>
        public int PermaformPopulation { get; set; } = 0;

        /// <summary>
        /// How much do new techs we just researched cost? 
        /// </summary>
        public float NewTechCostFactor { get; set; } = 1;

        /// <summary>
        /// How much do new techs we just researched cost? 
        /// </summary>
        public float MiniaturizationMax { get; set; } = .75f;

        /// <summary>
        /// How much cheaper do techs get as our tech levels improve? Defaults to 4% per level
        /// </summary>
        /// <value></value>
        public float MiniaturizationPerLevel { get; set; } = .04f;

        /// <summary>
        /// Are we able to build penetrating scanners at all?
        /// </summary>
        /// <value></value>
        public bool NoAdvancedScanners { get; set; } = false;

        /// <summary>
        /// Multiple all scan ranges by this factor
        /// </summary>
        public float ScanRangeFactor { get; set; } = 1f;

        /// <summary>
        /// Factor to multiply fuel usage by (i.e. .85 would make fuel usage 15% more efficient)
        /// </summary>
        /// <value></value>
        public float FuelEfficiencyOffset { get; set; } = 0f;

        /// <summary>
        /// This value will be added to the cost of terraforming. (it could reduce it or increase it, depending on the LRT)
        /// </summary>
        /// <returns></returns>
        public Cost TerraformCostOffset { get; set; } = new Cost();

        /// <summary>
        /// This value will be added to the resource cost of mineral alchemy. (it could reduce it or increase it, depending on the LRT)
        /// </summary>
        /// <returns></returns>
        public int MineralAlchemyCostOffset { get; set; } = 0;

        /// <summary>
        /// This will be added to the normal amount of minerals and resources returned after recycling
        /// </summary>
        /// <value></value>
        public float ScrapMineralOffset { get; set; } = 0f;

        /// <summary>
        /// By default, ships salvaged by starbases recover 80% of their minerals. This spec gives an offset
        /// to that
        /// </summary>
        /// <value></value>
        public float ScrapMineralOffsetStarbase { get; set; } = 0f;

        /// <summary>
        /// If true, scrapped fleets will return resources, not just minerals
        /// </summary>
        /// <value></value>
        public bool ScrapResources { get; set; }

        /// <summary>
        /// Factor to multiply the starting population of a planet by
        /// </summary>
        public float StartingPopulationFactor { get; set; } = 1f;

        /// <summary>
        /// Factor to multiply starbase cost by
        /// </summary>
        public float StarbaseCostFactor { get; set; } = 1f;

        /// <summary>
        /// ISB races have 20% built in cloaking for starbases
        /// </summary>
        public int StarbaseBuiltInCloakUnits { get; set; } = 0;

        /// <summary>
        /// Factor to multiply research resources by
        /// </summary>
        public float ResearchFactor { get; set; } = 1f;

        /// <summary>
        /// Factor of research resources to apply to other fields that aren't the current one (splash damage, get it?)
        /// </summary>
        public float ResearchSplashDamage { get; set; } = 0f;

        /// <summary>
        /// Amount to increase shield strength by
        /// </summary>
        /// <value></value>
        public float ShieldStrengthFactor { get; set; } = 1f;

        /// <summary>
        /// Rate per turn in battle that shields regenerate
        /// </summary>
        /// <value></value>
        public float ShieldRegenerationRate { get; set; } = 0f;

        /// <summary>
        /// Factor to multiply cost of engines by
        /// </summary>
        /// <value></value>
        public float EngineCostFactor { get; set; } = 1f;

        /// <summary>
        /// The rate at which engines fail, i.e. .1 is 10%
        /// </summary>
        /// <value></value>
        public float EngineFailureRate { get; set; } = 0f;

        /// <summary>
        /// The speed at which engines can reliably travel.
        /// </summary>
        /// <value></value>
        public float EngineReliableSpeed { get; set; } = 10;

        /// <summary>
        /// How good is this race at repairs
        /// </summary>
        public float RepairFactor { get; set; } = 1f;

        /// <summary>
        /// How good is this race at repairing starbases
        /// </summary>
        public float StarbaseRepairFactor { get; set; } = 1f;

    }
}