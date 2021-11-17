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

        public PermaformStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.PermaformStep)
        {
            this.planetService = planetService;
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
        }
    }
}