using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Apply leftover and allocated resources to research
    /// </summary>
    public class PlayerResearchStep : TurnGenerationStep
    {
        Researcher researcher = new Researcher();

        public PlayerResearchStep(Game game) : base(game, TurnGenerationState.Research) { }

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
                player.ResearchSpentLastYear = resourcesToSpend;
                researcher.ResearchNextLevel(player, resourcesToSpend);
            }

        }


    }
}