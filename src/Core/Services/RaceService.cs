using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class RaceService
    {
        static CSLog log = LogProvider.GetLogger(typeof(RaceService));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public RaceService(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }

        /// <summary>
        /// For a race, this will update the specs
        /// </summary>
        /// <param name="race"></param>
        public RaceSpec ComputeRaceSpecs(Race race)
        {
            // Create a race spec with our PRT and LRT features
            var spec = new RaceSpec();

            var prtSpec = Rules.PRTSpecs[race.PRT];
            spec.StartingTechLevels = prtSpec.StartingTechLevels;
            if (race.TechsStartHigh)
            {
                // Jack of All Trades start at 4
                var costsExtraLevel = prtSpec.TechsCostExtraLevel;
                foreach (TechField field in Enum.GetValues(typeof(TechField)))
                {
                    var level = spec.StartingTechLevels[field];
                    if (race.ResearchCost[field] == ResearchCostLevel.Extra && level < costsExtraLevel)
                    {
                        spec.StartingTechLevels[field] = costsExtraLevel;
                    }
                }
            }

            // some PRTs reduce max pop by half, others increase it by 20%
            spec.MaxPopulationOffset += prtSpec.MaxPopulationOffset;

            // go through all LRTs for the spec
            foreach (LRT lrt in race.LRTs)
            {
                var lrtSpec = Rules.LRTSpecs[lrt];
                spec.StartingTechLevels += lrtSpec.StartingTechLevels;

                spec.NewTechCostFactor *= lrtSpec.NewTechCostFactor;

                spec.TechCostReductionPercent *= lrtSpec.TechCostReductionPercent;
                spec.TechCostReductionPerLevel = Math.Max(spec.TechCostReductionPerLevel, lrtSpec.TechCostReductionPerLevel);
                spec.NoAdvancedScanners = spec.NoAdvancedScanners || lrtSpec.NoAdvancedScanners;
                spec.ScanRangeFactor *= lrtSpec.ScanRangeFactor;
                spec.FuelEfficiencyOffset += lrtSpec.FuelEfficiencyOffset;
                spec.MaxPopulationOffset += lrtSpec.MaxPopulationOffset;
                spec.TerraformCostOffset += lrtSpec.TerraformCostOffset;
                spec.MineralAlchemyCostOffset += lrtSpec.MineralAlchemyCostOffset;
                spec.ScrapMineralOffset += lrtSpec.ScrapMineralOffset;
                spec.ScrapMineralOffsetStarbase += lrtSpec.ScrapMineralOffsetStarbase;
                spec.ScrapResources = spec.ScrapResources || lrtSpec.ScrapResources;
                spec.StartingPopulationFactor *= lrtSpec.StartingPopulationFactor;
                spec.StarbaseCloakOffset += lrtSpec.StarbaseCloakOffset;
                spec.StarbaseCostFactor *= lrtSpec.StarbaseCostFactor;
                spec.ResearchFactor *= lrtSpec.ResearchFactor;
                spec.ResearchSplashDamage += lrtSpec.ResearchSplashDamage;
                spec.ShieldStrengthFactor *= lrtSpec.ShieldStrengthFactor;
                spec.ShieldRegenerationRate += lrtSpec.ShieldRegenerationRate;
                spec.EngineCostFactor *= lrtSpec.EngineCostFactor;
                spec.EngineFailureRate += lrtSpec.EngineFailureRate;
                spec.EngineReliableSpeed = Math.Min(spec.EngineReliableSpeed, lrtSpec.EngineReliableSpeed);

            }

            return spec;
        }
    }

}