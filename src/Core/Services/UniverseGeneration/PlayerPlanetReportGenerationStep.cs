using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Generate initial planet reports for the player. These are location and guid only, but those never change
    /// </summary>
    public class PlayerPlanetReportGenerationStep : UniverseGenerationStep
    {
        private readonly PlayerIntel playerIntel;

        public PlayerPlanetReportGenerationStep(IProvider<Game> gameProvider, PlayerIntel playerIntel) : base(gameProvider, UniverseGenerationState.PlanetReports)
        {
            this.playerIntel = playerIntel;
        }

        public override void Process()
        {
            Game.Planets.ForEach(planet =>
            {
                Game.Players.ForEach(player =>
                {
                    playerIntel.Discover(player, planet);
                });
            });
        }

    }
}