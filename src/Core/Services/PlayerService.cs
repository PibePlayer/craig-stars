using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class PlayerService
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerService));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public PlayerService(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }


        #region PRT and LRT logic

        public TechLevel GetStartingTechLevels(Race race)
        {
            TechLevel techLevels = Rules.PRTSpecs[race.PRT].StartingTechLevels.Clone();

            // if a race has Techs costing exra start high, set the start level to 3
            // for any TechField that is set to research costs extra
            if (race.TechsStartHigh)
            {
                // Jack of All Trades start at 4
                var costsExtraLevel = GetTechsCostExtraLevel(race);
                foreach (TechField field in Enum.GetValues(typeof(TechField)))
                {
                    var level = techLevels[field];
                    if (race.ResearchCost[field] == ResearchCostLevel.Extra && level < costsExtraLevel)
                    {
                        techLevels[field] = costsExtraLevel;
                    }
                }
            }

            if (race.HasLRT(LRT.IFE) || race.HasLRT(LRT.CE))
            {
                // Improved Fuel Efficiency and Cheap Engines increases propulsion by 1
                techLevels.Propulsion++;
            }

            return techLevels;
        }

        public List<StartingPlanet> GetStartingPlanets(Race race) => Rules.PRTSpecs[race.PRT].StartingPlanets;

        /// <summary>
        /// Get the starting fleets for a player
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        public List<StartingFleet> GetStartingFleets(Race race) => Rules.PRTSpecs[race.PRT].StartingFleets;

        /// <summary>
        /// Any population growth bonus this race gets, i.e. HE gets 2x growth
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public float GetGrowthFactor(Race race) => Rules.PRTSpecs[race.PRT].GrowthFactor;

        /// <summary>
        /// The max population factor for this race
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        public float GetMaxPopulationFactor(Race race) => Rules.PRTSpecs[race.PRT].MaxPopulationFactor + (race.HasLRT(LRT.OBRM) ? .1f : 0f);

        /// <summary>
        /// The rate this player's colonists grow on freighters, defaults to 0, but IS grows .5 * their growth rate
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public float GetFreighterGrowthFactor(Race race) => Rules.PRTSpecs[race.PRT].FreighterGrowthFactor;

        /// <summary>
        /// Does this race discover a ShipDesign's components on scan?
        /// </summary>
        /// <value></value>
        public bool DiscoverDesignOnScan(Race race) => Rules.PRTSpecs[race.PRT].DiscoverDesignOnScan;

        public Cost GetTerraformCost(Race race) => race.HasLRT(LRT.TT) ? Rules.TotalTerraformCost : Rules.TerraformCost;

        /// <summary>
        /// PP races can fling packets 1 warp faster without decaying.
        /// </summary>
        public float GetPacketDecayRate(Race race, MineralPacket packet)
        {
            int overSafeWarp = packet.WarpFactor - packet.SafeWarpSpeed;

            // IT is always count as being at least 1 over the safe warp
            overSafeWarp += Rules.PRTSpecs[race.PRT].PacketOverSafeWarpPenalty;

            // we only care about packets thrown up to 3 warp over the limit 
            overSafeWarp = Mathf.Clamp(packet.WarpFactor - packet.SafeWarpSpeed, 0, 3);

            var packetDecayRate = 0f;
            if (overSafeWarp > 0)
            {
                packetDecayRate = Rules.PacketDecayRate[overSafeWarp];
            }

            // PP have half the decay rate
            packetDecayRate *= Rules.PRTSpecs[race.PRT].PacketDecayFactor;

            return packetDecayRate;
        }

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        /// <value></value>
        public float GetPacketReceiverFactor(Race race) => Rules.PRTSpecs[race.PRT].PacketReceiverFactor;

        /// <summary>
        /// Get the cost to construct a single or mixed mineral packet 
        /// </summary>
        public int GetPacketResourceCost(Race race) => Rules.PRTSpecs[race.PRT].PacketResourceCost;

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        public float GetPacketCostFactor(Race race) => Rules.PRTSpecs[race.PRT].PacketMineralCostFactor;

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int GetMineralsPerMixedMineralPacket(Race race) => Rules.PRTSpecs[race.PRT].MineralsPerMixedMineralPacket;

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int GetMineralsPerSingleMineralPacket(Race race) => Rules.PRTSpecs[race.PRT].MineralsPerSingleMineralPacket;

        /// <summary>
        /// Can this player send cargo through Stargates
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        public bool CanGateCargo(Race race) => Rules.PRTSpecs[race.PRT].CanGateCargo;

        /// <summary>
        /// True if ships have a chance to vanish into the void when stargating
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        public bool ShipsVanishInVoid(Race race) => Rules.PRTSpecs[race.PRT].ShipsVanishInVoid;

        /// <summary>
        /// Does this player cloak ships with cargo without reducing the cloak percentage?
        /// </summary>
        public bool FreeCargoCloaking(Race race) => Rules.PRTSpecs[race.PRT].FreeCargoCloaking;

        /// <summary>
        /// The player's built in cloaking units
        /// </summary>
        public int BuiltInCloakUnits(Race race) => Rules.PRTSpecs[race.PRT].BuiltInCloakUnits;

        public int GetTechsCostExtraLevel(Race race) => Rules.PRTSpecs[race.PRT].TechsCostExtraLevel;

        public int GetBuiltInScannerMultiplier(Race race) => Rules.PRTSpecs[race.PRT].BuiltInScannerMultiplier;

        /// <summary>
        /// Do this player's minefields act like scanners?
        /// </summary>
        /// <value></value>
        public bool MineFieldsAreScanners(Race race) => Rules.PRTSpecs[race.PRT].MineFieldsAreScanners;

        public int GetMineFieldSafeWarpBonus(Race race) => Rules.PRTSpecs[race.PRT].MineFieldSafeWarpBonus;

        public float GetMineFieldMinDecayFactor(Race race) => Rules.PRTSpecs[race.PRT].MineFieldMinDecayFactor;

        public float GetMineFieldBaseDecayRate(Race race) => Rules.PRTSpecs[race.PRT].MineFieldBaseDecayRate;

        public float GetMineFieldPlanetDecayRate(Race race) => Rules.PRTSpecs[race.PRT].MineFieldPlanetDecayRate;

        public bool CanDetonateMineFields(Race race) => Rules.PRTSpecs[race.PRT].CanDetonateMineFields;

        public float GetMineFieldDetonateDecayRate(Race race) => Rules.PRTSpecs[race.PRT].MineFieldDetonateDecayRate;

        public float GetMineFieldMaxDecayRate(Race race) => Rules.PRTSpecs[race.PRT].MineFieldMaxDecayRate;

        /// <summary>
        /// Do this player's minefields act like scanners?
        /// </summary>
        /// <value></value>
        public bool FleetsReproduce(Race race) => Rules.PRTSpecs[race.PRT].FreighterGrowthFactor > 0;

        public bool CanRemoteMineOwnPlanets(Race race) => Rules.PRTSpecs[race.PRT].CanRemoteMineOwnPlanets;

        public float GetInvasionAttackBonus(Race race) => Rules.PRTSpecs[race.PRT].InvasionAttackBonus;
        public float GetInvasionDefendBonus(Race race) => Rules.PRTSpecs[race.PRT].InvasionDefendBonus;

        #endregion

        #region Production
        /// <summary>
        /// Get the cost of a single item in this ProductionQueueItem
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public Cost GetCostOfOne(Player player, ProductionQueueItem item)
        {
            var race = player.Race;
            int resources = 0;
            int germanium = 0;
            if (item.Type == QueueItemType.Mine || item.Type == QueueItemType.AutoMines)
            {
                resources = race.MineCost;
            }
            else if (item.Type == QueueItemType.Factory || item.Type == QueueItemType.AutoFactories)
            {
                resources = race.FactoryCost;
                germanium = Rules.FactoryCostGermanium;
                if (race.FactoriesCostLess)
                {
                    germanium = germanium - 1;
                }
            }
            else if (item.Type == QueueItemType.MineralAlchemy || item.Type == QueueItemType.AutoMineralAlchemy)
            {
                if (race.HasLRT(LRT.MA))
                {
                    resources = Rules.MineralAlchemyLRTCost;
                }
                else
                {
                    resources = Rules.MineralAlchemyCost;
                }
            }
            else if (item.Type == QueueItemType.Defenses || item.Type == QueueItemType.AutoDefenses)
            {
                return Rules.DefenseCost;
            }
            else if (item.Type == QueueItemType.TerraformEnvironment || item.Type == QueueItemType.AutoMaxTerraform || item.Type == QueueItemType.AutoMinTerraform)
            {
                return GetTerraformCost(player.Race);
            }
            else if (item.Type == QueueItemType.IroniumMineralPacket)
            {
                return new Cost(ironium: (int)(GetMineralsPerSingleMineralPacket(player.Race) * GetPacketCostFactor(player.Race)), resources: GetPacketResourceCost(player.Race));
            }
            else if (item.Type == QueueItemType.BoraniumMineralPacket)
            {
                return new Cost(boranium: (int)(GetMineralsPerSingleMineralPacket(player.Race) * GetPacketCostFactor(player.Race)), resources: GetPacketResourceCost(player.Race));
            }
            else if (item.Type == QueueItemType.GermaniumMineralPacket)
            {
                return new Cost(germanium: (int)(GetMineralsPerSingleMineralPacket(player.Race) * GetPacketCostFactor(player.Race)), resources: GetPacketResourceCost(player.Race));
            }
            else if (item.Type == QueueItemType.MixedMineralPacket || item.Type == QueueItemType.AutoMineralPacket)
            {
                float packetCostFactor = GetPacketCostFactor(player.Race);
                int mineralsPerMixedMineralPacket = GetMineralsPerMixedMineralPacket(player.Race);
                int packetResourceCost = GetPacketResourceCost(player.Race);

                return new Cost(
                    (int)(mineralsPerMixedMineralPacket * packetCostFactor),
                    (int)(mineralsPerMixedMineralPacket * packetCostFactor),
                    (int)(mineralsPerMixedMineralPacket * packetCostFactor),
                    GetPacketResourceCost(player.Race)
                );
            }
            else if (item.Type == QueueItemType.ShipToken || item.Type == QueueItemType.Starbase)
            {
                // ship designs have their own cost
                return item.Design.Aggregate.Cost;
            }

            return new Cost(germanium: germanium, resources: resources);
        }

        #endregion
    }

}