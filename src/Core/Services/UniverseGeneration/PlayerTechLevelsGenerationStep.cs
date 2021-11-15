using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Initialize players with their default tech levels
    /// </summary>
    public class PlayerTechLevelsGenerationStep : UniverseGenerationStep
    {
        private readonly PlayerService playerService;

        public PlayerTechLevelsGenerationStep(IProvider<Game> gameProvider, PlayerService playerService) : base(gameProvider, UniverseGenerationState.TechLevels) { 
            this.playerService = playerService;
        }

        public override void Process()
        {
            Game.Players.ForEach(player => player.TechLevels = player.Race.Spec.StartingTechLevels.Clone());
        }

    }
}