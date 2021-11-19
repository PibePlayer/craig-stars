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
        private readonly PlanetService planetService;
        private readonly Researcher researcher;

        public PlayerResearchStep(IProvider<Game> gameProvider, PlanetService planetService, Researcher researcher) : base(gameProvider, TurnGenerationState.PlayerResearchStep)
        {
            this.planetService = planetService;
            this.researcher = researcher;
        }

        public override void Process()
        {
            var planetsByPlayer = OwnedPlanets.GroupBy(p => p.PlayerNum);
            var totalSpent = new TechLevel();
            foreach (var playerPlanets in planetsByPlayer)
            {
                // figure out how many resources we have leftover
                var player = Game.Players[playerPlanets.Key];
                var resourcesToSpend = player.LeftoverResources;

                // figure out how many resoruces each planet allocates
                foreach (var planet in playerPlanets)
                {
                    resourcesToSpend += playerPlanets.Sum(p => planetService.GetResourcesPerYearResearch(p, player));
                }

                // research for this player
                player.ResearchSpentLastYear = resourcesToSpend;
                totalSpent += researcher.ResearchNextLevel(player, resourcesToSpend);
            }


            foreach (var player in Game.Players)
            {
                // find out if this player should steal any percentage of this research
                var stealsResearch = player.Race.Spec.StealsResearch;
                var stolenResearch = new TechLevel(
                    (int)(totalSpent.Energy * stealsResearch.Energy),
                    (int)(totalSpent.Weapons * stealsResearch.Weapons),
                    (int)(totalSpent.Propulsion * stealsResearch.Propulsion),
                    (int)(totalSpent.Construction * stealsResearch.Construction),
                    (int)(totalSpent.Electronics * stealsResearch.Electronics),
                    (int)(totalSpent.Biotechnology * stealsResearch.Biotechnology)
                );

                // we have stolen research! yay!
                // we steal the average of each research
                if (stolenResearch.Sum() > 0)
                {
                    foreach (TechField field in Enum.GetValues(typeof(TechField)))
                    {
                        var stolenResourcesForField = stolenResearch[field] / Game.Players.Count;
                        if (stolenResourcesForField > 0)
                        {
                            researcher.Research(player, stolenResourcesForField, field);
                        }
                    }
                }
            }

        }


    }
}