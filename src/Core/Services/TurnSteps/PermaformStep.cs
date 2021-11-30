using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Check if planets are permaformed
    /// </summary>
    public class PermaformStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(InstaformStep));
        private readonly PlanetService planetService;
        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public PermaformStep(IProvider<Game> gameProvider, PlanetService planetService, IRulesProvider rulesProvider) : base(gameProvider, TurnGenerationState.PermaformStep)
        {
            this.planetService = planetService;
            this.rulesProvider = rulesProvider;
        }

        public override void Process()
        {
            foreach (Planet planet in OwnedPlanets.Where(planet => Game.Players[planet.PlayerNum].Race.Spec.PermaformChance > 0))
            {
                Permaform(planet, Game.Players[planet.PlayerNum]);
            }
        }

        /// <summary>
        /// Terraform this planet one step in whatever the best option is
        /// </summary>
        /// <param name="planet"></param>
        internal void Permaform(Planet planet, Player player)
        {
            var adjustedPermaformChance = player.Race.Spec.PermaformChance;
            if (planet.Population <= player.Race.Spec.PermaformPopulation)
            {
                // adjust our permaform chance based on population size
                adjustedPermaformChance *= 1.0f - (player.Race.Spec.PermaformPopulation - planet.Population) / (float)player.Race.Spec.PermaformPopulation;
            }

            // see if we permaform
            if (adjustedPermaformChance >= (float)Rules.Random.NextDouble())
            {
                HabType habType = (HabType)Rules.Random.Next(3);
                var result = planetService.PermaformOneStep(planet, player, habType);

                if (result.Terraformed)
                {
                    Message.Permaform(player, planet, result.type, result.direction);
                }
            }
        }
    }
}