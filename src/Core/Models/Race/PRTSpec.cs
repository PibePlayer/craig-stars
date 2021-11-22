using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    public class PRTSpec : IPRTSpec
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

        public TechLevel StartingTechLevels { get; set; } = new();

        public List<StartingFleet> StartingFleets { get; set; } = new();

        public List<StartingPlanet> StartingPlanets { get; set; } = new List<StartingPlanet>()
        {
            // default with one planet, 25k people, no hab penalty
            new StartingPlanet(25000)
        };

        public Dictionary<TechCategory, float> TechCostFactor { get; set; } = new();

        [DefaultValue(100)]
        public int MineralsPerSingleMineralPacket { get; set; } = 100;

        [DefaultValue(40)]
        public int MineralsPerMixedMineralPacket { get; set; } = 40;

        [DefaultValue(10)]
        public int PacketResourceCost { get; set; } = 10;

        [DefaultValue(1.1f)]
        public float PacketMineralCostFactor { get; set; } = 1.1f;

        [DefaultValue(1f)]
        public float PacketReceiverFactor { get; set; } = 1f;

        [DefaultValue(1f)]
        public float PacketDecayFactor { get; set; } = 1f;

        [DefaultValue(0f)]
        public int PacketOverSafeWarpPenalty { get; set; } = 0;

        public bool PacketBuiltInScanner { get; set; }

        public bool DetectPacketDestinationStarbases { get; set; }

        public bool DetectAllPackets { get; set; }

        public float PacketTerraformChance { get; set; }

        public float PacketPermaformChance { get; set; }

        [DefaultValue(100)]
        public int PacketPermaformSizeUnit { get; set; }

        [DefaultValue(false)]
        public bool CanGateCargo { get; set; } = false;

        [DefaultValue(true)]
        public bool ShipsVanishInVoid { get; set; } = true;

        [DefaultValue(0)]
        public int BuiltInScannerMultiplier { get; set; } = 0;

        [DefaultValue(3)]
        public int TechsCostExtraLevel { get; set; } = 3;

        [DefaultValue(0f)]
        public float FreighterGrowthFactor { get; set; } = 0f;

        [DefaultValue(1f)]
        public float GrowthFactor { get; set; } = 1f;

        [DefaultValue(0f)]
        public float MaxPopulationOffset { get; set; } = 0f;

        [DefaultValue(0)]
        public int BuiltInCloakUnits { get; set; } = 0;

        public StealsResearch StealsResearch { get; set; } = new StealsResearch();

        public bool FreeCargoCloaking { get; set; } = false;

        public bool MineFieldsAreScanners { get; set; } = false;

        [DefaultValue(0)]
        public int MineFieldSafeWarpBonus { get; set; } = 0;

        [DefaultValue(1f)]
        public float MineFieldMinDecayFactor { get; set; } = 1f;

        [DefaultValue(.02f)]
        public float MineFieldBaseDecayRate { get; set; } = .02f;

        [DefaultValue(.04f)]
        public float MineFieldPlanetDecayRate { get; set; } = .04f;

        [DefaultValue(.5f)]
        public float MineFieldMaxDecayRate { get; set; } = .5f;

        [DefaultValue(false)]
        public bool CanDetonateMineFields { get; set; } = false;

        [DefaultValue(.25f)]
        public float MineFieldDetonateDecayRate { get; set; } = .25f;

        [DefaultValue(false)]
        public bool DiscoverDesignOnScan { get; set; } = false;

        [DefaultValue(false)]
        public bool CanRemoteMineOwnPlanets { get; set; } = false;

        [DefaultValue(1.1f)]
        public float InvasionAttackBonus { get; set; } = 1.1f;

        [DefaultValue(1f)]
        public float InvasionDefendBonus { get; set; } = 1f;

        [DefaultValue(0)]
        public int MovementBonus { get; set; } = 0;

        public bool Instaforming { get; set; } = false;

        public float PermaformChance { get; set; } = 0f;

        public int PermaformPopulation { get; set; } = 0;

        public float RepairFactor { get; set; } = 1f;

        public float StarbaseRepairFactor { get; set; } = 1f;

        public float StarbaseCostFactor { get; set; } = 1f;

    }
}