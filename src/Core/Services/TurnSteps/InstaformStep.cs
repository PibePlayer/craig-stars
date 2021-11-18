using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Instantly terraform planets (for races with Instaforming)
    /// </summary>
    public class InstaformStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(InstaformStep));
        private readonly PlanetService planetService;

        public InstaformStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.InstaformStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            foreach (Planet planet in OwnedPlanets.Where(planet => Game.Players[planet.PlayerNum].Race.Spec.Instaforming))
            {
                Instaform(planet, Game.Players[planet.PlayerNum]);
            }
        }

        /// <summary>
        /// Terraform this planet one step in whatever the best option is
        /// </summary>
        /// <param name="planet"></param>
        internal void Instaform(Planet planet, Player player)
        {
            Hab terraformAmount = planetService.GetTerraformAmount(planet, player);
            if (terraformAmount.AbsSum > 0)
            {
                // Instantly terraform this planet (but don't update planet.TerraformAmount, this change doesn't stick if we leave)
                planet.Hab = planet.Hab + terraformAmount;
                Message.Instaform(player, planet, terraformAmount);
            }
        }
    }
}