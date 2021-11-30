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
        public void LeftoverResourcesTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();

            var player = game.Players[0];
            var planet = game.Planets[0];

            // player1 researches with all resources
            planet.Population = 10000; // 10 resources
            planet.Factories = 10; // 10 resources
            planet.ProductionQueue.Items.Add(new ProductionQueueItem(QueueItemType.Mine, 1)); // 1 mine costs 5 resources, leaving 15
            player.ResearchAmount = 0;
            player.Researching = TechField.Energy;
            gameRunner.ComputeSpecs();

            PlanetProductionStep productionStep = new PlanetProductionStep(
                new Provider<Game>(game),
                TestUtils.TestContainer.GetInstance<PlanetService>(),
                TestUtils.TestContainer.GetInstance<PlayerService>(),
                TestUtils.TestContainer.GetInstance<FleetSpecService>()
            );

            // create a new research step for this game
            PlayerResearchStep researchStep = new PlayerResearchStep(
                new Provider<Game>(game),
                TestUtils.TestContainer.GetInstance<PlanetService>(),
                TestUtils.TestContainer.GetInstance<Researcher>()
            );

            // build
            productionStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // have leftover resources used for research, even though we apply 0%
            researchStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Energy]);
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

        [Test]
        public void GRResearchTest()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // research with 100 resources to make the math easy
            planet.Population = 100_000; // 100 resources
            planet.Factories = 0; // 0 resources
            player.ResearchAmount = 100;
            player.Researching = TechField.Energy;

            // add GR trait and make energy expensive (so our level doesn't go up)
            player.Race.LRTs.Add(LRT.GR);
            player.Race.ResearchCost[TechField.Energy] = ResearchCostLevel.Extra;
            player.Researching = TechField.Energy;
            gameRunner.ComputeSpecs(recompute: true);

            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // should spend half on the primary field
            Assert.AreEqual(50, player.TechLevelsSpent[TechField.Energy]);
            // 15% on other fields
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Weapons]);
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Propulsion]);
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Construction]);
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Electronics]);
            Assert.AreEqual(15, player.TechLevelsSpent[TechField.Biotechnology]);
        }
    }
}