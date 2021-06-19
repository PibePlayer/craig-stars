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
        /// This function will be called recursively until no more levels are passed
        /// From starsfaq
        ///   The cost of a tech level depends on four things: 
        ///  1) Your research setting for that field (cheap, normal, or expensive) 
        ///  2) The level you are researching (higher level, higher cost) 
        ///  3) The total number of tech levels you have already have in all fields (you can add it up yourself, or look at 'tech levels' on the 'score' screen). 
        ///  4) whether 'slow tech advance' was selected as a game parameter.
        ///
        ///  in general,
        ///
        ///  totalCost=(baseCost + (totalLevels * 10)) * costFactor
        ///
        ///  where  totalLevels=the sum of your current levels in all fields 
        ///    costFactor =.5 if your setting for the field is '50% less' 
        ///                       =1 if your setting for the field is 'normal' 
        ///                       =1.75 if your setting for the field is '75% more expensive'
        ///
        //  If 'slow tech advance' is a game parameter, totalCost should be doubled.
        ///
        ///  Below is a table showing the base cost of each level. 
        ///
        ///  1     50              14    18040 
        ///  2     80              15    22440 
        ///  3     130             16    27050 
        ///  4     210             17    31870 
        ///  5     340             18    36900 
        ///  6     550             19    42140 
        ///  7     890             20    47590 
        ///  8     1440            21    53250 
        ///  9     2330            22    59120 
        ///  10    3770            23    65200 
        ///  11    6100            24    71490 
        ///  12    9870            25    77990 
        ///  13    13850           26    84700
        /// 
        /// 
        /// </summary>
        /// <param name="resourcesToSpend">The amount of resources to spend on research</param>
        public void ResearchNextLevel(Player player, int resourcesToSpend)
        {
            if (!CheckMaxLevels(player))
            {
                return;
            }

            // don't research more than the max on this level
            var level = player.TechLevels[player.Researching];

            CheckMaxLevels(player);

            // add the resourcesToSpend to how much we've currently spent
            player.TechLevelsSpent[player.Researching] += resourcesToSpend;
            var spentOnCurrentLevel = player.TechLevelsSpent[player.Researching];

            int totalCost = GetTotalCost(player, player.Researching, level);

            if (spentOnCurrentLevel >= totalCost)
            {
                // increase a level
                player.TechLevels[player.Researching]++;

                // figure out how many leftover points we have
                var leftoverResources = spentOnCurrentLevel - totalCost;

                // reset the amount we spent to zero
                player.TechLevelsSpent[player.Researching] = 0;

                // determine the next field to research
                var nextField = GetNextResearchField(player);

                // notify our player (and any listeners) that we got a new level
                var newLevel = player.TechLevels[player.Researching];
                Message.TechLevel(player, player.Researching, newLevel, nextField);
                EventManager.PublishPlayerResearchLevelIncreasedEvent(player, player.Researching, newLevel);

                // setup the next level
                player.Researching = nextField;

                if (leftoverResources > 0)
                {
                    // we have leftover resources, so call ourselves again
                    // to apply them to the next level
                    ResearchNextLevel(player, leftoverResources);
                }
            }
        }

        /// <summary>
        /// Validate that a player can research their current field and switch them if not
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true if this player can research at all, false if they are maxed out in levels</returns>
        internal bool CheckMaxLevels(Player player)
        {
            var maxTechLevel = player.Rules.TechBaseCost.Length - 1;
            if (player.TechLevels[player.Researching] >= maxTechLevel)
            {
                // determine the next field to research
                var nextField = GetNextResearchField(player);
                if (player.TechLevels[nextField] >= maxTechLevel)
                {
                    nextField = player.TechLevels.Lowest();
                }

                if (player.TechLevels[nextField] >= maxTechLevel)
                {
                    Message.Info(player, $"You have researched all there is to research. No further research will be done.");
                    player.Researching = TechField.Energy;
                    return false;
                }
                else
                {
                    Message.Info(player, $"You have completed all possible research in the {player.Researching} field. Your scientists will focus on {nextField} next.");
                    player.Researching = nextField;
                }
            }

            return true;
        }

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

        /// <summary>
        /// Get the next TechField to research based on the NextResearchField setting
        /// </summary>
        /// <returns></returns>
        public TechField GetNextResearchField(Player player) => player.NextResearchField switch
        {
            NextResearchField.Energy => TechField.Energy,
            NextResearchField.Weapons => TechField.Weapons,
            NextResearchField.Propulsion => TechField.Propulsion,
            NextResearchField.Construction => TechField.Construction,
            NextResearchField.Electronics => TechField.Electronics,
            NextResearchField.Biotechnology => TechField.Biotechnology,
            NextResearchField.LowestField => player.TechLevels.Lowest(),
            _ => player.Researching
        };

    }
}

