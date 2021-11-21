using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
namespace CraigStars
{
    public class LRTSpec : ILRTSpec
    {
        [JsonConstructor]
        public LRTSpec(LRT lrt, int pointCost)
        {
            LRT = lrt;
            PointCost = pointCost;
        }

        [DefaultValue(LRT.None)]
        public LRT LRT { get; }
        public int PointCost { get; set; }

        public TechLevel StartingTechLevels { get; set; } = new();
        public float NewTechCostFactor { get; set; } = 1f;
        public float MiniaturizationMax { get; set; } = .75f;
        public float MiniaturizationPerLevel { get; set; } = .04f;
        public bool NoAdvancedScanners { get; set; } = false;
        public float ScanRangeFactor { get; set; } = 1f;
        public float FuelEfficiencyOffset { get; set; } = 0f;
        public float MaxPopulationOffset { get; set; } = 0f;
        public Cost TerraformCostOffset { get; set; } = new Cost();
        public int MineralAlchemyCostOffset { get; set; } = 0;
        public float ScrapMineralOffset { get; set; } = 0f;
        public float ScrapMineralOffsetStarbase { get; set; } = 0f;
        public bool ScrapResources { get; set; }
        public float StartingPopulationFactor { get; set; } = 1f;
        public int StarbaseBuiltInCloakUnits { get; set; }
        public float StarbaseCostFactor { get; set; } = 1f;
        public float ResearchFactor { get; set; } = 1f;
        public float ResearchSplashDamage { get; set; } = 0f;
        public float ShieldStrengthFactor { get; set; } = 1f;
        public float ShieldRegenerationRate { get; set; } = 0f;
        public float EngineCostFactor { get; set; } = 1f;
        public float EngineFailureRate { get; set; } = 0f;
        public float EngineReliableSpeed { get; set; } = 10;
    }
}