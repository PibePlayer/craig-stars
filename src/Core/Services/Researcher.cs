using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// This service manages researching techs for players
    /// </summary>
    public class Researcher
    {
        static CSLog log = LogProvider.GetLogger(typeof(Researcher));

        /// <summary>
        /// Get the total cost for this player to research a given level of tech
        /// Tech gets more expensive the more total tech levels you have
        /// </summary>
        /// <param name="player"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetTotalCost(Player player, TechField field, int level)
        {
            // we can't research more than tech level 26
            if (level >= 26)
            {
                return 0;
            }

            // figure out our total levels
            var totalLevels = player.TechLevels.Sum();

            // figure out the cost to advance to the next level
            var baseCost = player.Rules.TechBaseCost[level + 1];
            var researchCost = player.Race.ResearchCost[field];
            var costFactor = 1f;
            switch (researchCost)
            {
                case ResearchCostLevel.Extra:
                    costFactor = 1.75f;
                    break;
                case ResearchCostLevel.Less:
                    costFactor = .5f;
                    break;
            }

            // from starsfaq
            int totalCost = (int)((baseCost + (totalLevels * 10)) * costFactor);

            return totalCost;
        }
    }
}

