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
    public class InstaformStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(InstaformStepTest));

        Game game;
        GameRunner gameRunner;
        PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
        InstaformStep step;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new InstaformStep(gameRunner.GameProvider, planetService);
        }

        [Test]
        public void TestInstaform()
        {
            var player = game.Players[0];
            player.Race.PRT = PRT.CA;
            // allow Grav3 terraform
            player.TechLevels = new TechLevel(propulsion: 1, biotechnology: 1);

            var planet = new Planet()
            {
                PlayerNum = player.Num,
                Hab = new Hab(45, 50, 50),
                BaseHab = new Hab(45, 50, 50),
                TerraformedAmount = new Hab(),
            };

            game.Planets.Add(planet);
            gameRunner.ComputeSpecs();

            // should terraform 3 grav points
            step.Instaform(planet, player);
            Assert.AreEqual(new Hab(48, 50, 50), planet.Hab);

            // make sure we lose instaforming on planet empty
            planetService.EmptyPlanet(planet);
            Assert.AreEqual(new Hab(45, 50, 50), planet.Hab);
        }
    }
}