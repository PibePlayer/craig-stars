using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// If we have a GameStartModifier set, grow planets, do research, add mines and factories, etc.
    /// </summary>
    public class GameStartModeModifier
    {
        public void AdvanceGame(Game game)
        {
            switch (game.Rules.StartMode)
            {
                case GameStartMode.MidGame:
                case GameStartMode.LateGame:
                case GameStartMode.EndGame:
                    AdvanceToMidGame(game);
                    break;
            }
        }

        void AdvanceToMidGame(Game game)
        {
            int growthYears = 20;
            int techBonus = 15000; // resources to spend on research.
            int mineFactoryBonus = 500;
            int defenseBonus = 90;
            var ownedPlanets = game.OwnedPlanets.ToList();

            PlayerResearchStep playerResearchStep = new PlayerResearchStep(game, TurnGeneratorState.Research);
            playerResearchStep.PreProcess(ownedPlanets);
            game.Players.ForEach(player =>
            {
                playerResearchStep.ResearchNextLevel(player, techBonus);
            });

            PlanetGrowStep planetGrowStep = new PlanetGrowStep(game, TurnGeneratorState.Grow);
            planetGrowStep.PreProcess(ownedPlanets);
            for (int i = 0; i < growthYears; i++)
            {
                planetGrowStep.Process();
            }

            // build some mines
            ownedPlanets.ForEach(planet =>
            {
                planet.Mines = Mathf.Clamp(planet.Mines + mineFactoryBonus, 0, planet.MaxPossibleMines);
                planet.Factories = Mathf.Clamp(planet.Factories + mineFactoryBonus, 0, planet.MaxPossibleFactories);
                planet.Defenses = Mathf.Clamp(planet.Defenses + defenseBonus, 0, planet.MaxDefenses);
            });

        }
    }
}