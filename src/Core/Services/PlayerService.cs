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

        /// <summary>
        /// Any population growth bonus this race gets, i.e. HE gets 2x growth
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public float GetGrowthFactor(Player player) => player.Race.PRT == PRT.HE ? Rules.HEGrowthFactor : 1.0f;
        
        /// <summary>
        /// The rate this player's colonists grow on freighters, defaults to 0, but IS grows .5 * their growth rate
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public float GetFreighterGrowthFactor(Player player) => player.Race.PRT == PRT.IS ? Rules.ISFreighterGrowthFactor : 0.0f;

        /// <summary>
        /// Does this race discover a ShipDesign's components on scan?
        /// </summary>
        /// <value></value>
        public bool DiscoverDesignOnScan(Race race) => race.PRT == PRT.WM;

        public Cost GetTerraformCost(Race race) => race.HasLRT(LRT.TT) ? Rules.TotalTerraformCost : Rules.TerraformCost;

        /// <summary>
        /// PP races can fling packets 1 warp faster without decaying.
        /// </summary>
        public float GetPacketDecayRate(Race race, MineralPacket packet)
        {
            int overSafeWarp = packet.WarpFactor - packet.SafeWarpSpeed;

            if (race.PRT == PRT.IT)
            {
                // IT is always count as being at least 1 over the safe warp
                overSafeWarp++;
            }

            // we only care about packets thrown up to 3 warp over the limit 
            overSafeWarp = Mathf.Clamp(packet.WarpFactor - packet.SafeWarpSpeed, 0, 3);

            var packetDecayRate = 0f;
            if (overSafeWarp > 0)
            {
                packetDecayRate = Rules.PacketDecayRate[overSafeWarp];
            }

            if (race.PRT == PRT.PP)
            {
                // PP have half the decay rate
                packetDecayRate *= .5f;
            }

            return packetDecayRate;
        }

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        /// <value></value>
        public float GetPacketReceiverFactor(Race race) => race.PRT == PRT.IT ? .5f : 1f;

        /// <summary>
        /// Get the cost to construct a single or mixed mineral packet 
        /// </summary>
        public int GetPacketResourceCost(Race race) => race.PRT switch
        {
            PRT.PP => Rules.PacketResourceCostPP,
            _ => Rules.PacketResourceCost
        };

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        public float GetPacketCostFactor(Race race) => race.PRT switch
        {
            PRT.PP => Rules.PacketMineralCostFactorPP,
            PRT.IT => Rules.PacketMineralCostFactorIT,
            _ => Rules.PacketMineralCostFactor
        };

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int GetMineralsPerMixedMineralPacket(Race race) => race.PRT switch
        {
            PRT.PP => Rules.MineralsPerMixedMineralPacketPP,
            _ => Rules.MineralsPerMixedMineralPacket
        };

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int GetMineralsPerSingleMineralPacket(Race race) => race.PRT switch
        {
            PRT.PP => Rules.MineralsPerSingleMineralPacketPP,
            _ => Rules.MineralsPerSingleMineralPacket
        };

        /// <summary>
        /// Does this player cloak ships with cargo without reducing the cloak percentage?
        /// </summary>
        public bool FreeCargoCloaking(Race race) => race.PRT == PRT.SS;

        /// <summary>
        /// The player's built in cloaking percentage
        /// </summary>
        public int BuiltInCloaking(Race race) => race.PRT == PRT.SS ? Rules.BuiltInSSCloakUnits : 0;

        /// <summary>
        /// Do this player's minefields act like scanners?
        /// </summary>
        /// <value></value>
        public bool MineFieldsAreScanners(Race race) => race.PRT == PRT.SD;

        /// <summary>
        /// Do this player's minefields act like scanners?
        /// </summary>
        /// <value></value>
        public bool FleetsReproduce(Race race) => race.PRT == PRT.IS;


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