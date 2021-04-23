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
        public PlayerPlanetReportGenerationStep(Game game) : base(game, UniverseGenerationState.PlanetReports) { }

        PlayerIntel playerIntel = new PlayerIntel();

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