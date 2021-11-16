using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class InstaformStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(InstaformStep));
        private readonly PlanetService planetService;

        public InstaformStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.Production)
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
                // for example, the planet has Grav 49, but our player wants Grav 50 
                planet.Hab = planet.Hab + terraformAmount;
                Message.Instaform(player, planet, terraformAmount);
            }
        }
    }
}