using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class CheckVictoryStepTest
    {
        PlanetService planetService;
        PlayerIntelDiscoverer playerIntelDiscoverer;

        [SetUp]
        public void SetUp()
        {
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            playerIntelDiscoverer = TestUtils.TestContainer.GetInstance<PlayerIntelDiscoverer>();
        }

        [Test]
        public void TestCheckOwnPlanets()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            // we must own >= 51% of the planets
            game.VictoryConditions.OwnPlanets = 51;

            // this player owns all planets (1)
            var step = new CheckVictoryStep(gameRunner.GameProvider, planetService);
            step.CheckOwnPlanets(player);

            Assert.IsTrue(player.AchievedVictoryConditions.Contains(VictoryConditionType.OwnPlanets));

            // add a planet, now our player only owns half the planets
            game.AddMapObject(new Planet() { Name = "Planet 2" });
            player.AchievedVictoryConditions.Clear();
            step.CheckOwnPlanets(player);
            Assert.IsFalse(player.AchievedVictoryConditions.Contains(VictoryConditionType.OwnPlanets));
        }

        [Test]
        public void TestCheckExceedSecondPlaceScore()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            player1.Score.Score = 201;
            player2.Score.Score = 100;

            // we must exceed the second player score by 100%
            game.VictoryConditions.ExceedsScore = 100;

            // this player owns all planets
            var step = new CheckVictoryStep(gameRunner.GameProvider, planetService);
            step.CheckExceedSecondPlaceScore(player1);

            Assert.IsTrue(player1.AchievedVictoryConditions.Contains(VictoryConditionType.ExceedsSecondPlaceScore));

            // we only exceed the score by 20%
            player1.AchievedVictoryConditions.Clear();
            player1.Score.Score = 120;
            step.CheckExceedSecondPlaceScore(player1);
            Assert.IsFalse(player1.AchievedVictoryConditions.Contains(VictoryConditionType.ExceedsSecondPlaceScore));
        }

    }
}