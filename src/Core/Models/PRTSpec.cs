using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    public record StartingFleet(string Name, string HullName, ShipDesignPurpose Purpose) { }
    public record StartingPlanet(
        int Population,
        float HabPenaltyFactor = 0,
        bool HasStargate = false,
        bool HasMassDriver = false,
        List<StartingFleet> StartingFleets = null
    )
    { }

    public record StealsResearch(
        float Energy = 0f,
        float Weapons = 0f,
        float Propulsion = 0f,
        float Construction = 0f,
        float Electronics = 0f,
        float Biotechnology = 0f
    )
    {
        public float this[TechField index]
        {
            get
            {
                switch (index)
                {
                    case TechField.Energy:
                        return Energy;
                    case TechField.Weapons:
                        return Weapons;
                    case TechField.Propulsion:
                        return Propulsion;
                    case TechField.Construction:
                        return Construction;
                    case TechField.Electronics:
                        return Electronics;
                    case TechField.Biotechnology:
                        return Biotechnology;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }
    }

    /// <summary>
    /// The specification for a PRT
    /// </summary>
    public class PRTSpec
    {
        [JsonConstructor]
        public PRTSpec(PRT prt, int pointCost)
        {
            PRT = prt;
            PointCost = pointCost;
        }

        [DefaultValue(PRT.None)]
        public PRT PRT { get; }

        public int PointCost { get; set; } = 66;

        /// <summary>
        /// Bonus starting tech levels this trait brings
        /// </summary>
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
        /// The amount of minerals in a single ironium, boranium, or germanium packet
        /// </summary>
        /// <value></value>
        [DefaultValue(100)]
        public int MineralsPerSingleMineralPacket { get; set; } = 100;

        /// <summary>
        /// The amount of minerals in a mixed (all minerals at once) packet
        /// </summary>
        /// <value></value>
        [DefaultValue(40)]
        public int MineralsPerMixedMineralPacket { get; set; } = 40;

        /// <summary>
        /// The cost, in resources, to create packets
        /// </summary>
        [DefaultValue(10)]
        public int PacketResourceCost { get; set; } = 10;

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        [DefaultValue(1.1f)]
        public float PacketMineralCostFactor { get; set; } = 1.1f;

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        [DefaultValue(1f)]
        public float PacketReceiverFactor { get; set; } = 1f;

        /// <summary>
        /// Factor impacting the decay rate for packets. Some races packets decay faster
        /// </summary>
        [DefaultValue(1f)]
        public float PacketDecayFactor { get; set; } = 1f;

        /// <summary>
        /// Some races are so bad at flinging packets, their safe warp is always worse
        /// </summary>
        /// <value></value>
        [DefaultValue(0f)]
        public int PacketOverSafeWarpPenalty { get; set; } = 0;

        /// <summary>
        /// Can this rate send cargo through stargates?
        /// </summary>
        [DefaultValue(false)]
        public bool CanGateCargo { get; set; } = false;

        /// <summary>
        /// Does this race lose ships to the endless void if they overgate?
        /// </summary>
        [DefaultValue(true)]
        public bool ShipsVanishInVoid { get; set; } = true;

        /// <summary>
        /// Some races can operate the built in scanners on certain hulls
        /// </summary>
        [DefaultValue(0)]
        public int BuiltInScannerMultiplier { get; set; } = 0;

        /// <summary>
        /// The level all techs that "cost extra" to research start at
        /// </summary>
        [DefaultValue(3)]
        public int TechsCostExtraLevel { get; set; } = 3;

        /// <summary>
        /// The rate of growth for colonists on freighters
        /// </summary>
        [DefaultValue(0f)]
        public float FreighterGrowthFactor { get; set; } = 0f;

        /// <summary>
        /// Multiplier impacting base growth rate (i.e. HE races are 2x)
        /// </summary>
        [DefaultValue(1f)]
        public float GrowthFactor { get; set; } = 1f;

        /// <summary>
        /// Bonus to max pop, in percent. I.e. .1f would give a 10% bonus to max pop. this field stacks
        /// with LRTs
        /// </summary>
        [DefaultValue(0f)]
        public float MaxPopulationOffset { get; set; } = 0f;

        /// <summary>
        /// SS races have 300 cloak units built in (or 75% cloaking)
        /// </summary>
        [DefaultValue(0)]
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
        [DefaultValue(0)]
        public int MineFieldSafeWarpBonus { get; set; } = 0;

        /// <summary>
        /// The minimum rate minefields decay
        /// </summary>
        [DefaultValue(1f)]
        public float MineFieldMinDecayFactor { get; set; } = 1f;

        /// <summary>
        /// The base rate minefields decay
        /// </summary>
        [DefaultValue(.02f)]
        public float MineFieldBaseDecayRate { get; set; } = .02f;

        /// <summary>
        /// The rate minefields decay per planet they surround
        /// </summary>
        [DefaultValue(.04f)]
        public float MineFieldPlanetDecayRate { get; set; } = .04f;

        /// <summary>
        /// The max rate minefields will decay
        /// </summary>
        [DefaultValue(.5f)]
        public float MineFieldMaxDecayRate { get; set; } = .5f;

        /// <summary>
        /// Can this race detonate minefields
        /// </summary>
        [DefaultValue(false)]
        public bool CanDetonateMineFields { get; set; } = false;

        /// <summary>
        /// The number of mines that are lost to detonation each turn
        /// </summary>
        [DefaultValue(.25f)]
        public float MineFieldDetonateDecayRate { get; set; } = .25f;

        /// <summary>
        /// Does this race discover details about a fleet design when scanning? (this is normally only discovered in battle)
        /// </summary>
        [DefaultValue(false)]
        public bool DiscoverDesignOnScan { get; set; } = false;

        /// <summary>
        /// Can this race remote mine their own planets?
        /// </summary>
        [DefaultValue(false)]
        public bool CanRemoteMineOwnPlanets { get; set; } = false;

        /// <summary>
        /// The factor that invaders get when invading planets
        /// </summary>
        [DefaultValue(1.1f)]
        public float InvasionAttackBonus { get; set; } = 1.1f;

        /// <summary>
        /// The factor defenders get when defending from planet invasion
        /// </summary>
        [DefaultValue(1f)]
        public float InvasionDefendBonus { get; set; } = 1f;

        /// <summary>
        /// Increase in battle movement for this race
        /// </summary>
        [DefaultValue(0)]
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
        public int PermaformPopAdjust { get; set; } = 0;

    }
}