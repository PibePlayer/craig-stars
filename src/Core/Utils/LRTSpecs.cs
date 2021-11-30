using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CraigStars
{
    /// <summary>
    /// The specifications for built in PRTs
    /// </summary>
    public class LRTSpecs
    {
        public List<LRTSpec> Specs { get; set; } = new List<LRTSpec>()
        {
            LRTSpecs.IFE,
            LRTSpecs.TT,
            LRTSpecs.ARM,
            LRTSpecs.ISB,
            LRTSpecs.GR,
            LRTSpecs.UR,
            LRTSpecs.NRSE,
            LRTSpecs.OBRM,
            LRTSpecs.NAS,
            LRTSpecs.LSP,
            LRTSpecs.BET,
            LRTSpecs.RS,
            LRTSpecs.MA,
            LRTSpecs.CE
        };

        /// <summary>
        /// Get a LRTSpec by prt (this is just an index lookup)
        /// </summary>
        /// <returns></returns>
        public LRTSpec this[LRT lrt] => Specs[(int)lrt];

        public static LRTSpec IFE = new LRTSpec(LRT.IFE, 0)
        {
            StartingTechLevels = new TechLevel(propulsion: 1),
            FuelEfficiencyOffset = -.15f,
        };

        public static LRTSpec TT = new LRTSpec(LRT.TT, 0)
        {
            TerraformCostOffset = new Cost(resources: -30),
        };

        public static LRTSpec ARM = new LRTSpec(LRT.ARM, 0)
        {

        };

        public static LRTSpec ISB = new LRTSpec(LRT.ISB, 0)
        {
            StarbaseBuiltInCloakUnits = 40, // 20% built in cloaking
            StarbaseCostFactor = .8f
        };

        public static LRTSpec GR = new LRTSpec(LRT.GR, 0)
        {
            ResearchFactor = .5f,
            ResearchSplashDamage = .15f
        };

        public static LRTSpec UR = new LRTSpec(LRT.UR, 0)
        {
            // UR gives us 45% of scrapped minerals and resources, versus 1/3 for races without UR
            ScrapMineralOffset = .45f - (1f / 3f),
            ScrapMineralOffsetStarbase = .1f,
            ScrapResources = true,
        };

        public static LRTSpec NRSE = new LRTSpec(LRT.NRSE, 0)
        {
        };

        public static LRTSpec OBRM = new LRTSpec(LRT.OBRM, 0)
        {
            MaxPopulationOffset = .1f,
        };

        public static LRTSpec NAS = new LRTSpec(LRT.NAS, 0)
        {
            NoAdvancedScanners = true,
            ScanRangeFactor = 2f,
        };

        public static LRTSpec LSP = new LRTSpec(LRT.LSP, 0)
        {
            StartingPopulationFactor = .7f,
        };

        public static LRTSpec BET = new LRTSpec(LRT.BET, 0)
        {
            NewTechCostFactor = 2,
            MiniaturizationMax = .8f,
            MiniaturizationPerLevel = .05f,
        };

        public static LRTSpec RS = new LRTSpec(LRT.RS, 0)
        {
            ShieldStrengthFactor = 1.4f,
            ShieldRegenerationRate = .1f
        };

        public static LRTSpec MA = new LRTSpec(LRT.MA, 0)
        {
            MineralAlchemyCostOffset = -75
        };

        public static LRTSpec CE = new LRTSpec(LRT.CE, 0)
        {
            StartingTechLevels = new TechLevel(propulsion: 1),

            TechCostOffset = new Dictionary<TechCategory, float>() {
                { TechCategory.Engine, -.5f }, // engines cost 50% less
            },

            EngineFailureRate = .1f,
            EngineReliableSpeed = 6
        };

    }
}