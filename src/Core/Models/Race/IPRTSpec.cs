using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The specification for a PRT
    /// </summary>
    public interface IPRTSpec
    {
        /// <summary>
        /// Bonus starting tech levels this trait brings
        /// </summary>
        TechLevel StartingTechLevels { get; set; }

        /// <summary>
        /// Starting fleets this trait includes
        /// </summary>
        List<StartingFleet> StartingFleets { get; set; }

        /// <summary>
        /// Spec of starting planets this trait includes
        /// </summary>
        List<StartingPlanet> StartingPlanets { get; set; }

        /// <summary>
        /// The cost factor this rate applies to various TechCategories
        /// </summary>
        /// <returns></returns>
        Dictionary<TechCategory, float> TechCostOffset { get; set; }

        /// <summary>
        /// The amount of minerals in a single ironium, boranium, or germanium packet
        /// </summary>
        /// <value></value>
        [DefaultValue(100)]
        int MineralsPerSingleMineralPacket { get; set; }

        /// <summary>
        /// The amount of minerals in a mixed (all minerals at once) packet
        /// </summary>
        /// <value></value>
        [DefaultValue(40)]
        int MineralsPerMixedMineralPacket { get; set; }

        /// <summary>
        /// The cost, in resources, to create packets
        /// </summary>
        [DefaultValue(10)]
        int PacketResourceCost { get; set; }

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        [DefaultValue(1.1f)]
        float PacketMineralCostFactor { get; set; }

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        [DefaultValue(1f)]
        float PacketReceiverFactor { get; set; }

        /// <summary>
        /// Factor impacting the decay rate for packets. Some races packets decay faster
        /// </summary>
        [DefaultValue(1f)]
        float PacketDecayFactor { get; set; }

        /// <summary>
        /// Some races are so bad at flinging packets, their safe warp is always worse
        /// </summary>
        /// <value></value>
        [DefaultValue(0f)]
        int PacketOverSafeWarpPenalty { get; set; }

        /// <summary>
        /// Do packets have a built in scanner?
        /// </summary>
        bool PacketBuiltInScanner { get; set; }

        /// <summary>
        /// Do packets detect the destination starbase when they arrive?
        /// </summary>
        bool DetectPacketDestinationStarbases { get; set; }

        /// <summary>
        /// Does this race detect all packets in space?
        /// </summary>
        /// <value></value>
        bool DetectAllPackets { get; set; }

        /// <summary>
        /// Chance this race's packets terraform the receiving planet in this race's favor
        /// </summary>
        /// <value></value>
        float PacketTerraformChance { get; set; }

        /// <summary>
        /// Chance that this race's packets permaform the receiving planet in this race's favor
        /// This only applies to every PacketPermaformSizeUnit kT of uncaught resources
        /// </summary>
        /// <value></value>
        float PacketPermaformChance { get; set; }

        /// <summary>
        /// The unit of uncaught packet size for permaforming, i.e. every 100kT of uncaught minerals
        /// has a .1% chance of permaforming the planet
        /// </summary>
        /// <value></value>
        [DefaultValue(100)]
        int PacketPermaTerraformSizeUnit { get; set; }

        /// <summary>
        /// Can this rate send cargo through stargates?
        /// </summary>
        bool CanGateCargo { get; set; }

        /// <summary>
        /// Can this race detect other planets with stargates if they are in range?
        /// </summary>
        /// <value></value>
        bool CanDetectStargatePlanets { get; set; }

        /// <summary>
        /// Does this race lose ships to the endless void if they overgate?
        /// </summary>
        [DefaultValue(true)]
        bool ShipsVanishInVoid { get; set; }

        /// <summary>
        /// Some races can operate the built in scanners on certain hulls
        /// </summary>
        [DefaultValue(0)]
        int BuiltInScannerMultiplier { get; set; }

        /// <summary>
        /// The level all techs that "cost extra" to research start at
        /// </summary>
        [DefaultValue(3)]
        int TechsCostExtraLevel { get; set; }

        /// <summary>
        /// The rate of growth for colonists on freighters
        /// </summary>
        [DefaultValue(0f)]
        float FreighterGrowthFactor { get; set; }

        /// <summary>
        /// Multiplier impacting base growth rate (i.e. HE races are 2x)
        /// </summary>
        [DefaultValue(1f)]
        float GrowthFactor { get; set; }

        /// <summary>
        /// Bonus to max pop, in percent. I.e. .1f would give a 10% bonus to max pop. this field stacks
        /// with LRTs
        /// </summary>
        [DefaultValue(0f)]
        float MaxPopulationOffset { get; set; }

        /// <summary>
        /// SS races have 300 cloak units built in (or 75% cloaking)
        /// </summary>
        [DefaultValue(0)]
        int BuiltInCloakUnits { get; set; }

        /// <summary>
        /// SS races steal research
        /// </summary>
        StealsResearch StealsResearch { get; set; }

        /// <summary>
        /// Does this race cloak cargo for free? (i.e. cargo doesn't count towards mass for cloak calcs)
        /// </summary>
        bool FreeCargoCloaking { get; set; }

        /// <summary>
        /// Do minefields act as scanners?
        /// </summary>
        bool MineFieldsAreScanners { get; set; }

        /// <summary>
        /// The factor to multiply the minefield laying rate by while the ship moves
        /// SD races can lay half mines while moving, all other races lay none while moving
        /// </summary>
        /// <value></value>
        float MineFieldRateMoveFactor { get; set; }

        /// <summary>
        /// Space Demolition fleets can travel 2 warp speeds faster through minefields
        /// </summary>
        [DefaultValue(0)]
        int MineFieldSafeWarpBonus { get; set; }

        /// <summary>
        /// The minimum rate minefields decay
        /// </summary>
        [DefaultValue(1f)]
        float MineFieldMinDecayFactor { get; set; }

        /// <summary>
        /// The base rate minefields decay
        /// </summary>
        [DefaultValue(.02f)]
        float MineFieldBaseDecayRate { get; set; }

        /// <summary>
        /// The rate minefields decay per planet they surround
        /// </summary>
        [DefaultValue(.04f)]
        float MineFieldPlanetDecayRate { get; set; }

        /// <summary>
        /// The max rate minefields will decay
        /// </summary>
        [DefaultValue(.5f)]
        float MineFieldMaxDecayRate { get; set; }

        /// <summary>
        /// Can this race detonate minefields
        /// </summary>
        [DefaultValue(false)]
        bool CanDetonateMineFields { get; set; }

        /// <summary>
        /// The number of mines that are lost to detonation each turn
        /// </summary>
        [DefaultValue(.25f)]
        float MineFieldDetonateDecayRate { get; set; }

        /// <summary>
        /// Does this race discover details about a fleet design when scanning? (this is normally only discovered in battle)
        /// </summary>
        [DefaultValue(false)]
        bool DiscoverDesignOnScan { get; set; }

        /// <summary>
        /// Can this race remote mine their own planets?
        /// </summary>
        [DefaultValue(false)]
        bool CanRemoteMineOwnPlanets { get; set; }

        /// <summary>
        /// The factor that invaders get when invading planets
        /// </summary>
        [DefaultValue(1.1f)]
        float InvasionAttackBonus { get; set; }

        /// <summary>
        /// The factor defenders get when defending from planet invasion
        /// </summary>
        [DefaultValue(1f)]
        float InvasionDefendBonus { get; set; }

        /// <summary>
        /// Increase in battle movement for this race
        /// </summary>
        [DefaultValue(0)]
        int MovementBonus { get; set; }

        /// <summary>
        /// Does the race instaform new worlds
        /// </summary>
        /// <value></value>
        bool Instaforming { get; set; }

        /// <summary>
        /// The chance for permaforming a planet, assuming population is >= PermaformPopulation
        /// </summary>
        /// <value></value>
        float PermaformChance { get; set; }

        /// <summary>
        /// The Population where a race's permaform chances top out
        /// </summary>
        /// <value></value>
        int PermaformPopulation { get; set; }

        /// <summary>
        /// How good is this race at repairs
        /// </summary>
        float RepairFactor { get; set; }

        /// <summary>
        /// How good is this race at repairing starbases
        /// </summary>
        float StarbaseRepairFactor { get; set; }

        /// <summary>
        /// The factor to multiply the total cost of a starbase by
        /// Note: this is not cumulative with the ISB LRT
        /// </summary>
        [DefaultValue(1f)]
        float StarbaseCostFactor { get; set; }

        /// <summary>
        /// Does this race have innate mining (because they live in space)
        /// </summary>
        /// <value></value>
        bool InnateMining { get; set; }

        /// <summary>
        /// Does this race have innate resources (because they can't build factories)
        /// </summary>
        /// <value></value>
        bool InnateResources { get; set; }

        /// <summary>
        /// Does this race have innate scanning (because they live in space)
        /// </summary>
        /// <value></value>
        bool InnateScanner { get; set; }

        /// <summary>
        /// Can this race build planetary defenses
        /// </summary>
        /// <value></value>
        [DefaultValue(true)]
        bool CanBuildDefenses { get; set; }

        /// <summary>
        /// Does this race live on starbases (as opposed to planets?)
        /// </summary>
        /// <value></value>
        bool LivesOnStarbases { get; set; }

    }
}