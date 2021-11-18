using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using FakeItEasy;

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
    public class PermaformStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PermaformStepTest));

        Game game;
        GameRunner gameRunner;
        PermaformStep step;
        PlanetService planetService;
        IRulesProvider mockRulesProvider;

        [SetUp]
        public void SetUp()
        {
            mockRulesProvider = A.Fake<IRulesProvider>();
            // make sure we use this single rules provider so we can mock it
            planetService = new PlanetService(
            TestUtils.TestContainer.GetInstance<PlayerService>(),
            TestUtils.TestContainer.GetInstance<PlayerTechService>(),
            mockRulesProvider);

            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new PermaformStep(gameRunner.GameProvider, planetService, mockRulesProvider);
        }

        [Test]
        public void TestPermaform()
        {
            var player = game.Players[0];
            player.Race.PRT = PRT.CA;

            gameRunner.ComputeAggregates();

            // make this 100%
            player.Race.Spec.PermaformChance = .1f;
            player.Race.Spec.PermaformPopulation = 100_000;

            var planet = new Planet()
            {
                PlayerNum = player.Num,
                Population = 100_000,
                Hab = new Hab(45, 50, 50),
                BaseHab = new Hab(45, 50, 50),
                TerraformedAmount = new Hab(),
            };

            // make sure our random number generator returns "yes, permaform" and "permaform gravity"
            var mockRules = A.Fake<Rules>();
            var random = A.Fake<Random>();
            A.CallTo(() => mockRulesProvider.Rules).Returns(mockRules);
            A.CallTo(() => mockRules.Random).Returns(random);
            A.CallTo(() => random.Next(3)).Returns((int)HabType.Gravity);

            // should permaform one step, and adjust our value up one
            step.Permaform(planet, player);
            Assert.AreEqual(new Hab(46, 50, 50), planet.Hab);
            Assert.AreEqual(new Hab(46, 50, 50), planet.BaseHab);

            // if we are already perfect, the hab should stay the same but base hab should go up
            planet.BaseHab = new Hab(45, 50, 50);
            planet.Hab = new Hab(50, 50, 50);
            step.Permaform(planet, player);
            Assert.AreEqual(new Hab(50, 50, 50), planet.Hab);
            Assert.AreEqual(new Hab(46, 50, 50), planet.BaseHab);

            // no permaforming if random chance doesn't happen
            planet.BaseHab = new Hab(45, 50, 50);
            planet.Hab = new Hab(45, 50, 50);
            A.CallTo(() => random.NextDouble()).Returns(.2);
            step.Permaform(planet, player);
            Assert.AreEqual(new Hab(45, 50, 50), planet.Hab);
            Assert.AreEqual(new Hab(45, 50, 50), planet.BaseHab);

            // no permaforming if we are already good
            planet.BaseHab = new Hab(45, 50, 50);
            planet.Hab = new Hab(45, 50, 50);
            A.CallTo(() => random.NextDouble()).Returns(.1);
            A.CallTo(() => random.Next(3)).Returns((int)HabType.Temperature);
            step.Permaform(planet, player);
            Assert.AreEqual(new Hab(45, 50, 50), planet.Hab);
            Assert.AreEqual(new Hab(45, 50, 50), planet.BaseHab);

        }
    }
}