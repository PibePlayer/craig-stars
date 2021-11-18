using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Threading.Tasks;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerResearchStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerResearchStepTest));

        PlayerResearchStep step;
        Game game;
        GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            step = new PlayerResearchStep(
                new Provider<Game>(game),
                TestUtils.TestContainer.GetInstance<PlanetService>(),
                TestUtils.TestContainer.GetInstance<Researcher>()
            );
        }

        [Test]
        public void StolenResearchTest()
        {
            var player1 = game.Players[0];
            var planet1 = game.Planets[0];
            var player2 = game.Players[1];
            var planet2 = game.Planets[1];

            // player1 researches with all resources
            planet1.Population = 10000; // 10 resources
            planet1.Factories = 10; // 10 resources
            player1.ResearchAmount = 100;
            player1.Researching = TechField.Energy;

            // player2 researches with all resources
            planet2.Population = 10000;
            planet2.Factories = 10;
            player2.ResearchAmount = 100;
            player2.Researching = TechField.Electronics;

            // player2 steals resources
            player2.Race.PRT = PRT.SS;
            gameRunner.ComputeSpecs();

            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());
            Assert.AreEqual(20, player1.TechLevelsSpent[TechField.Energy]);
            // 5 stolen (average of 10 + 0 / 2 player)
            Assert.AreEqual(5, player2.TechLevelsSpent[TechField.Energy]);
            // 20 for us, 5 stolen ... from ourselves!
            Assert.AreEqual(25, player2.TechLevelsSpent[TechField.Electronics]);
        }
    }
}