using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Apply leftover and allocated resources to research
    /// </summary>
    public class PlayerResearchStep : Step
    {
        public PlayerResearchStep(Game game) : base(game, TurnGeneratorState.Research) { }

        public override void Process()
        {
            var planetsByPlayer = OwnedPlanets.GroupBy(p => p.Player);
            foreach (var playerPlanets in planetsByPlayer)
            {
                // figure out how many resoruces each planet has
                var resourcesToSpend = 0;
                foreach (var planet in playerPlanets)
                {
                    resourcesToSpend += playerPlanets.Sum(p => p.ResourcesPerYearResearch);
                }

                // research for this player
                var player = playerPlanets.Key;
                ResearchNextLevel(player, resourcesToSpend);
            }

        }

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
        internal void ResearchNextLevel(Player player, int resourcesToSpend)
        {
            // add the resourcesToSpend to how much we've currently spent
            player.TechLevelsSpent[player.Researching] += resourcesToSpend;
            var spentOnCurrentLevel = player.TechLevelsSpent[player.Researching];

            // don't research more than the max on this level
            // TODO: If we get to max level, automatically switch to the lowest field
            var level = player.TechLevels[player.Researching];
            if (level >= Game.Rules.TechBaseCost.Length - 1)
            {
                Message.Info(player, $"You are already at level {level} in {player.Researching} and cannot research further.");
                return;
            }

            // figure out our total levels
            var totalLevels = player.TechLevels.Sum();

            // figure out the cost to advance to the next level
            var baseCost = Game.Rules.TechBaseCost[level + 1];
            var researchCost = player.Race.ResearchCost[player.Researching];
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
        /// Get the next TechField to research based on the NextResearchField setting
        /// </summary>
        /// <returns></returns>
        public TechField GetNextResearchField(Player player)
        {
            var nextField = player.Researching;
            switch (player.NextResearchField)
            {
                case NextResearchField.Energy:
                    nextField = TechField.Energy;
                    break;
                case NextResearchField.Weapons:
                    nextField = TechField.Weapons;
                    break;
                case NextResearchField.Propulsion:
                    nextField = TechField.Propulsion;
                    break;
                case NextResearchField.Construction:
                    nextField = TechField.Construction;
                    break;
                case NextResearchField.Electronics:
                    nextField = TechField.Electronics;
                    break;
                case NextResearchField.Biotechnology:
                    nextField = TechField.Biotechnology;
                    break;
                case NextResearchField.LowestField:
                    nextField = player.TechLevels.Lowest();
                    break;
            }

            return nextField;
        }
    }
}