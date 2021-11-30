using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Interface for an LRTSpec
    /// </summary>
    public interface ILRTSpec
    {
        /// <summary>
        /// Bonus starting tech levels this trait brings
        /// </summary>
        TechLevel StartingTechLevels { get; set; }

        /// <summary>
        /// The cost factor this rate applies to various TechCategories
        /// </summary>
        /// <returns></returns>
        Dictionary<TechCategory, float> TechCostOffset { get; set; }

        /// <summary>
        /// How much do new techs we just researched cost? 
        /// </summary>
        float NewTechCostFactor { get; set; }

        /// <summary>
        /// What is the highest cost discount we can achieve for
        /// </summary>
        float MiniaturizationMax { get; set; }

        /// <summary>
        /// How much cheaper do techs get as our tech levels improve? Defaults to 4% per level
        /// </summary>
        /// <value></value>
        float MiniaturizationPerLevel { get; set; }

        /// <summary>
        /// Are we able to build penetrating scanners at all?
        /// </summary>
        /// <value></value>
        bool NoAdvancedScanners { get; set; }

        /// <summary>
        /// Multiple all scan ranges by this factor
        /// </summary>
        float ScanRangeFactor { get; set; }

        /// <summary>
        /// Factor to multiply fuel usage by (i.e. .85 would make fuel usage 15% more efficient)
        /// </summary>
        /// <value></value>
        float FuelEfficiencyOffset { get; set; }

        /// <summary>
        /// Percent bonus to increase max population by
        /// </summary>
        /// <value></value>
        float MaxPopulationOffset { get; set; }

        /// <summary>
        /// This value will be added to the cost of terraforming. (it could reduce it or increase it, depending on the LRT)
        /// </summary>
        /// <returns></returns>
        Cost TerraformCostOffset { get; set; }

        /// <summary>
        /// This value will be added to the resource cost of mineral alchemy. (it could reduce it or increase it, depending on the LRT)
        /// </summary>
        /// <returns></returns>
        int MineralAlchemyCostOffset { get; set; }

        /// <summary>
        /// This will be added to the normal amount of minerals and resources returned after recycling
        /// </summary>
        /// <value></value>
        float ScrapMineralOffset { get; set; }

        /// <summary>
        /// By default, ships salvaged by starbases recover 80% of their minerals. This spec gives an offset
        /// to that
        /// </summary>
        /// <value></value>
        float ScrapMineralOffsetStarbase { get; set; }

        /// <summary>
        /// If true, scrapped fleets will return resources, not just minerals
        /// </summary>
        /// <value></value>
        bool ScrapResources { get; set; }

        /// <summary>
        /// Factor to multiply the starting population of a planet by
        /// </summary>
        float StartingPopulationFactor { get; set; }

        /// <summary>
        /// ISB races have 20% built in cloaking for starbases
        /// </summary>
        int StarbaseBuiltInCloakUnits { get; set; }

        /// <summary>
        /// Factor to multiply starbase cost by
        /// </summary>
        float StarbaseCostFactor { get; set; }

        /// <summary>
        /// Factor to multiply research resources by
        /// </summary>
        float ResearchFactor { get; set; }

        /// <summary>
        /// Factor of research resources to apply to other fields that aren't the current one (splash damage, get it?)
        /// </summary>
        float ResearchSplashDamage { get; set; }

        /// <summary>
        /// Amount to increase shield strength by
        /// </summary>
        /// <value></value>
        float ShieldStrengthFactor { get; set; }

        /// <summary>
        /// Rate per turn in battle that shields regenerate
        /// </summary>
        /// <value></value>
        float ShieldRegenerationRate { get; set; }

        /// <summary>
        /// The rate at which engines fail, i.e. .1 is 10%
        /// </summary>
        /// <value></value>
        float EngineFailureRate { get; set; }

        /// <summary>
        /// The speed at which engines can reliably travel.
        /// </summary>
        /// <value></value>
        float EngineReliableSpeed { get; set; }
    }
}