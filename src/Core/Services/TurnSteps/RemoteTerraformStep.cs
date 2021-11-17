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

        public RemoteTerraformStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.RemoteTerraformStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
        }

        /// <summary>
        /// Remote terraform this planet, for good or for ill
        /// </summary>
        /// <param name="planet"></param>
        internal void RemoteTerraform(Planet planet, Fleet fleet, Player terraformer, Player planetOwner)
        {
        }
    }
}