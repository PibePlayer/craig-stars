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

            // copy all PRT specs over
            spec.StartingFleets = new(prtSpec.StartingFleets);
            spec.StartingPlanets = new(prtSpec.StartingPlanets);
            spec.TechCostFactor = new(prtSpec.TechCostFactor);

            // PP
            spec.MineralsPerSingleMineralPacket = prtSpec.MineralsPerSingleMineralPacket;
            spec.MineralsPerMixedMineralPacket = prtSpec.MineralsPerMixedMineralPacket;
            spec.PacketResourceCost = prtSpec.PacketResourceCost;
            spec.PacketMineralCostFactor = prtSpec.PacketMineralCostFactor;
            spec.PacketReceiverFactor = prtSpec.PacketReceiverFactor;
            spec.PacketDecayFactor = prtSpec.PacketDecayFactor;
            spec.PacketDecayFactor = prtSpec.PacketDecayFactor;
            spec.PacketBuiltInScanner = prtSpec.PacketBuiltInScanner;
            spec.DetectPacketDestinationStarbases = prtSpec.DetectPacketDestinationStarbases;
            spec.DetectAllPackets = prtSpec.DetectAllPackets;
            spec.PacketTerraformChance = prtSpec.PacketTerraformChance;
            spec.PacketPermaformChance = prtSpec.PacketPermaformChance;
            spec.PacketPermaTerraformSizeUnit = prtSpec.PacketPermaTerraformSizeUnit;

            // IT
            spec.PacketOverSafeWarpPenalty = prtSpec.PacketOverSafeWarpPenalty;
            spec.CanGateCargo = prtSpec.CanGateCargo;
            spec.CanDetectStargatePlanets = prtSpec.CanDetectStargatePlanets;
            spec.ShipsVanishInVoid = prtSpec.ShipsVanishInVoid;

            // JoaT
            spec.BuiltInScannerMultiplier = prtSpec.BuiltInScannerMultiplier;
            spec.TechsCostExtraLevel = prtSpec.TechsCostExtraLevel;

            // IS
            spec.FreighterGrowthFactor = prtSpec.FreighterGrowthFactor;
            spec.InvasionDefendBonus = prtSpec.InvasionDefendBonus;
            spec.RepairFactor = prtSpec.RepairFactor;
            spec.StarbaseRepairFactor = prtSpec.StarbaseRepairFactor;

            // HE
            spec.GrowthFactor = prtSpec.GrowthFactor;

            // SS
            spec.BuiltInCloakUnits = prtSpec.BuiltInCloakUnits;
            spec.StealsResearch = prtSpec.StealsResearch;
            spec.FreeCargoCloaking = prtSpec.FreeCargoCloaking;

            // SD
            spec.MineFieldsAreScanners = prtSpec.MineFieldsAreScanners;
            spec.MineFieldSafeWarpBonus = prtSpec.MineFieldSafeWarpBonus;
            spec.MineFieldMinDecayFactor = prtSpec.MineFieldMinDecayFactor;
            spec.MineFieldBaseDecayRate = prtSpec.MineFieldBaseDecayRate;
            spec.MineFieldPlanetDecayRate = prtSpec.MineFieldPlanetDecayRate;
            spec.MineFieldMaxDecayRate = prtSpec.MineFieldMaxDecayRate;
            spec.CanDetonateMineFields = prtSpec.CanDetonateMineFields;
            spec.MineFieldDetonateDecayRate = prtSpec.MineFieldDetonateDecayRate;

            // WM
            spec.DiscoverDesignOnScan = prtSpec.DiscoverDesignOnScan;
            spec.InvasionAttackBonus = prtSpec.InvasionAttackBonus;

            // AR
            spec.CanRemoteMineOwnPlanets = prtSpec.CanRemoteMineOwnPlanets;
            spec.MovementBonus = prtSpec.MovementBonus;
            spec.StarbaseCostFactor = prtSpec.StarbaseCostFactor;

            // CA
            spec.Instaforming = prtSpec.Instaforming;
            spec.PermaformChance = prtSpec.PermaformChance;
            spec.PermaformPopulation = prtSpec.PermaformPopulation;


            // some PRTs reduce max pop by half, others increase it by 20%
            // HE & JoaT
            spec.MaxPopulationOffset += prtSpec.MaxPopulationOffset;

            // go through all LRTs for the spec
            foreach (LRT lrt in race.LRTs)
            {
                var lrtSpec = Rules.LRTSpecs[lrt];
                spec.StartingTechLevels += lrtSpec.StartingTechLevels;

                spec.NewTechCostFactor *= lrtSpec.NewTechCostFactor;

                spec.MiniaturizationMax *= lrtSpec.MiniaturizationMax;
                spec.MiniaturizationPerLevel = Math.Max(spec.MiniaturizationPerLevel, lrtSpec.MiniaturizationPerLevel);
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
                spec.StarbaseBuiltInCloakUnits += lrtSpec.StarbaseBuiltInCloakUnits;
                // This only gets set once. This means two LRTs with a StarbaseCostFactor will pick the first one though...
                spec.StarbaseCostFactor = spec.StarbaseCostFactor == 1f ? Math.Min(spec.StarbaseCostFactor, lrtSpec.StarbaseCostFactor) : spec.StarbaseCostFactor;
                spec.ResearchFactor *= lrtSpec.ResearchFactor;
                spec.ResearchSplashDamage += lrtSpec.ResearchSplashDamage;
                spec.ShieldStrengthFactor *= lrtSpec.ShieldStrengthFactor;
                spec.ShieldRegenerationRate += lrtSpec.ShieldRegenerationRate;
                spec.EngineCostFactor *= lrtSpec.EngineCostFactor;
                spec.EngineFailureRate += lrtSpec.EngineFailureRate;
                spec.EngineReliableSpeed = Math.Min(spec.EngineReliableSpeed, lrtSpec.EngineReliableSpeed);

            }

            spec.Costs = GetCostsForItems(race, spec);

            return spec;
        }

        /// <summary>
        /// Get a dictionary of costs by item type for this race
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        Dictionary<QueueItemType, Cost> GetCostsForItems(Race race, RaceSpec spec) => new()
        {
            { QueueItemType.Mine, new Cost(resources: race.MineCost) },
            { QueueItemType.AutoMines, new Cost(resources: race.MineCost) },
            { QueueItemType.Factory, new Cost(germanium: Rules.FactoryCostGermanium + (race.FactoriesCostLess ? -1 : 0), resources: race.FactoryCost) },
            { QueueItemType.AutoFactories, new Cost(germanium: Rules.FactoryCostGermanium + (race.FactoriesCostLess ? -1 : 0), resources: race.FactoryCost) },
            { QueueItemType.MineralAlchemy, new Cost(resources: Rules.MineralAlchemyCost + spec.MineralAlchemyCostOffset) },
            { QueueItemType.AutoMineralAlchemy, new Cost(resources: Rules.MineralAlchemyCost + spec.MineralAlchemyCostOffset) },
            { QueueItemType.Defenses, Rules.DefenseCost },
            { QueueItemType.AutoDefenses, Rules.DefenseCost },
            { QueueItemType.TerraformEnvironment, Rules.TerraformCost + spec.TerraformCostOffset },
            { QueueItemType.AutoMaxTerraform, Rules.TerraformCost + spec.TerraformCostOffset },
            { QueueItemType.AutoMinTerraform, Rules.TerraformCost + spec.TerraformCostOffset },
            { QueueItemType.IroniumMineralPacket, new Cost(resources: spec.PacketResourceCost, ironium: (int)(spec.MineralsPerSingleMineralPacket * spec.PacketMineralCostFactor)) },
            { QueueItemType.BoraniumMineralPacket, new Cost(resources: spec.PacketResourceCost, boranium: (int)(spec.MineralsPerSingleMineralPacket * spec.PacketMineralCostFactor)) },
            { QueueItemType.GermaniumMineralPacket, new Cost(resources: spec.PacketResourceCost, germanium: (int)(spec.MineralsPerSingleMineralPacket * spec.PacketMineralCostFactor)) },
            {
                QueueItemType.MixedMineralPacket,
                new Cost(
                    resources: spec.PacketResourceCost,
                    ironium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor),
                    boranium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor),
                    germanium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor)
                )
            },
            {
                QueueItemType.AutoMineralPacket,
                new Cost(
                    resources: spec.PacketResourceCost,
                    ironium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor),
                    boranium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor),
                    germanium: (int)(spec.MineralsPerMixedMineralPacket * spec.PacketMineralCostFactor)
                )
            },
        };

    }

}