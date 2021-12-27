using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Generate initial planet reports for the player. These are location and guid only, but those never change
    /// </summary>
    public class PlayerPlanetReportGenerationStep : UniverseGenerationStep
    {
        private readonly PlayerIntelDiscoverer playerIntelDiscoverer;

        public PlayerPlanetReportGenerationStep(IProvider<Game> gameProvider, PlayerIntelDiscoverer playerIntelDiscoverer) : base(gameProvider, UniverseGenerationState.PlanetReports)
        {
            this.playerIntelDiscoverer = playerIntelDiscoverer;
        }

        public override void Process()
        {
            Game.Planets.ForEach(planet =>
            {
                Game.Players.ForEach(player =>
                {
                    playerIntelDiscoverer.Discover(player, planet);
                });
            });
        }

    }
}