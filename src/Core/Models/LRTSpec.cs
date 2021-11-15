using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The specification for an LRT
    /// </summary>
    public class LRTSpec
    {
        [JsonConstructor]
        public LRTSpec(LRT lrt, int pointCost)
        {
            LRT = lrt;
            PointCost = pointCost;
        }

        [DefaultValue(LRT.None)]
        public LRT LRT { get; }

        public int PointCost { get; set; } = 66;

        public TechLevel StartingTechLevels { get; set; } = new();

        /// <summary>
        /// How much do new techs we just researched cost? 
        /// </summary>
        public float NewTechCostFactor = 1f;

        /// <summary>
        /// How much do new techs we just researched cost? 
        /// </summary>
        public float TechCostReductionPercent { get; set; } = .75f;

        /// <summary>
        /// How much cheaper do techs get as our tech levels improve? Defaults to 4% per level
        /// </summary>
        /// <value></value>
        public float TechCostReductionPerLevel { get; set; } = .04f;

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
        /// Percent bonus to increase max population by
        /// </summary>
        /// <value></value>
        public float MaxPopulationOffset { get; set; } = 0f;

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
        /// Amount to offset Starbase Cloaks by
        /// </summary>
        public float StarbaseCloakOffset { get; set; } = 0;

        /// <summary>
        /// Factor to multiply starbase cost by
        /// </summary>
        public float StarbaseCostFactor { get; set; } = 1f;

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
    }
}