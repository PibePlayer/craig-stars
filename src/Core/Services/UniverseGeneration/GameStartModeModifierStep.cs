using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// If we have a GameStartModifier set, grow planets, do research, add mines and factories, etc.
    /// </summary>
    public class GameStartModeModifierStep : UniverseGenerationStep
    {
        private readonly PlanetService planetService;
        private readonly PlanetGrowStep planetGrowStep;
        private readonly Researcher researcher;

        public GameStartModeModifierStep(IProvider<Game> gameProvider, PlanetService planetService, PlanetGrowStep planetGrowStep, Researcher researcher) : base(gameProvider, UniverseGenerationState.GameStartMode)
        {
            this.planetService = planetService;
            this.planetGrowStep = planetGrowStep;
            this.researcher = researcher;
        }

        public override void Process()
        {
            switch (Game.GameInfo.StartMode)
            {
                case GameStartMode.MidGame:
                case GameStartMode.LateGame:
                case GameStartMode.EndGame:
                    AdvanceToMidGame(Game);
                    break;
            }
        }

        void AdvanceToMidGame(Game game)
        {
            int growthYears = 20;
            int techBonus = 40000; // resources to spend on research.
            int mineFactoryBonus = 500;
            int defenseBonus = 90;
            var ownedPlanets = game.OwnedPlanets.ToList();

            game.Players.ForEach(player =>
            {
                researcher.ResearchNextLevel(player, techBonus);
            });

            planetGrowStep.PreProcess(ownedPlanets);
            for (int i = 0; i < growthYears; i++)
            {
                planetGrowStep.Process();
            }

            // build some mines
            ownedPlanets.ForEach(planet =>
            {
                var player = game.Players[planet.PlayerNum];
                planet.Mines = Mathf.Clamp(planet.Mines + mineFactoryBonus, 0, planetService.GetMaxPossibleMines(planet, player));
                planet.Factories = Mathf.Clamp(planet.Factories + mineFactoryBonus, 0, planetService.GetMaxPossibleFactories(planet, player));
                planet.Defenses = Mathf.Clamp(planet.Defenses + defenseBonus, 0, planetService.GetMaxDefenses(planet, player));
            });



        }
    }
}