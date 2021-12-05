using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Check if planets are permaformed
    /// </summary>
    public class RemoteTerraformStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(InstaformStep));
        private readonly PlanetService planetService;
        private readonly PlayerTechService playerTechService;

        public RemoteTerraformStep(IProvider<Game> gameProvider, PlanetService planetService, PlayerTechService playerTechService) : base(gameProvider, TurnGenerationState.RemoteTerraformStep)
        {
            this.planetService = planetService;
            this.playerTechService = playerTechService;
        }

        public override void Process()
        {
            // for any fleets with remote terraforming abilities that are orbiting planets owned by someone other than us
            foreach (var fleet in Game.Fleets.Where(fleet =>
                fleet.Spec.TerraformRate > 0
                && fleet.Orbiting != null
                && fleet.Orbiting.Owned
                && !fleet.Orbiting.OwnedBy(fleet.PlayerNum)))
            {
                RemoteTerraform(fleet.Orbiting, fleet, Game.Players[fleet.PlayerNum], Game.Players[fleet.Orbiting.PlayerNum]);
            }
        }

        /// <summary>
        /// Remote terraform this planet, for good or for ill
        /// </summary>
        /// <param name="planet"></param>
        internal void RemoteTerraform(Planet planet, Fleet fleet, Player terraformer, Player planetOwner)
        {
            int amountToTerraform = fleet.Spec.TerraformRate;

            for (int i = 0; i < amountToTerraform; i++)
            {
                bool enemy = terraformer.IsEnemy(planetOwner.Num);
                planetService.TerraformOneStep(planet, planetOwner, terraformer, reverse: enemy);
            }
        }

        /// <summary>
        /// Get the terraform ability of a player taking into account total terraform and hav terraform
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Hab GetTerraformAbility(Player player)
        {
            TechTerraform bestTotalTerraform = playerTechService.GetBestTerraform(player, TerraformHabType.All);
            int totalTerraformAbility = bestTotalTerraform == null ? 0 : bestTotalTerraform.Ability;

            Hab terraformAbility = new Hab(totalTerraformAbility, totalTerraformAbility, totalTerraformAbility);

            foreach (HabType habType in Enum.GetValues(typeof(HabType)))
            {
                // get the two ways we can terraform
                TechTerraform bestHabTerraform = playerTechService.GetBestTerraform(player, (TerraformHabType)habType);

                // find out which terraform tech has the greater terraform ability
                int ability = totalTerraformAbility;
                if (bestHabTerraform != null)
                {
                    ability = Mathf.Max(ability, bestHabTerraform.Ability);
                    terraformAbility = terraformAbility.WithType(habType, ability);
                }

                if (ability == 0)
                {
                    continue;
                }
            }

            return terraformAbility;
        }


    }
}