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
        PlanetService planetService = new();
        Researcher researcher = new();

        public PlayerResearchStep(Game game) : base(game, TurnGenerationState.Research) { }

        public override void Process()
        {
            var planetsByPlayer = OwnedPlanets.GroupBy(p => p.PlayerNum);
            foreach (var playerPlanets in planetsByPlayer)
            {
                // figure out how many resoruces each planet has
                var resourcesToSpend = 0;
                foreach (var planet in playerPlanets)
                {
                    var player = Game.Players[planet.PlayerNum];
                    resourcesToSpend += playerPlanets.Sum(p => planetService.GetResourcesPerYearResearch(p, player));
                }

                // research for this player
                var playerNum = playerPlanets.Key;
                Game.Players[playerNum].ResearchSpentLastYear = resourcesToSpend;
                researcher.ResearchNextLevel(Game.Players[playerNum], resourcesToSpend);
            }

        }


    }
}