using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

namespace CraigStars.UniverseGeneration
{
    public class UniverseGenerator
    {
        Game Game { get; }

        List<UniverseGenerationStep> steps = new List<UniverseGenerationStep>();

        PlayerIntel playerIntel = new PlayerIntel();

        public UniverseGenerator(Game game)
        {
            Game = game;
            steps.AddRange(new List<UniverseGenerationStep>()
            {
                new PlanetGenerationStep(Game),
                new WormholeGenerationStep(Game),
                new PlayerTechLevelsGenerationStep(Game),
                new PlayerPlansGenerationStep(Game),
                new PlayerShipDesignsGenerationStep(Game),
                new PlayerPlanetReportGenerationStep(Game),
                new PlayerHomeworldGenerationStep(Game),
                new PlayerFleetGenerationStep(Game),
                new GameStartModeModifierStep(Game),
        });
        }

        public void Generate()
        {
            foreach (var step in steps)
            {
                step.Process();
            }

            Game.Players.ForEach(player => player.SetupMapObjectMappings());

        }





    }
}