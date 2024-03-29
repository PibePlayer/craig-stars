using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class InvasionProcessorTest
    {
        PlanetService planetService;
        InvasionProcessor invasionProcessor;

        [SetUp]
        public void SetUp()
        {
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            invasionProcessor = new InvasionProcessor(
               planetService,
               TestUtils.TestContainer.GetInstance<IRulesProvider>()
           );
        }

        [Test]
        public void TestInvadePlanetNoDefenses()
        {
            var planetOwner = new Player()
            {
                Num = 0
            };
            var fleetOwner = new Player()
            {
                Num = 1
            };

            var planet = new Planet()
            {
                Name = "Brin",
                PlayerNum = planetOwner.Num,
                Population = 10000,
                Mines = 100,
                Factories = 100,
                Defenses = 0,
            };

            // one mini-bomber
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Teamster #1",
            };

            fleet.Orbiting = planet;

            // 10000 attackers for 10000 undefended defenders, attacker wins
            int attackers = 10000;
            invasionProcessor.InvadePlanet(planet, planetOwner, fleetOwner, fleet, attackers);

            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
            Assert.AreEqual(900, planet.Population);
            Assert.AreEqual(0, planet.Defenses);
            Assert.AreEqual(fleetOwner.Num, planet.PlayerNum);

            Assert.AreEqual(100, planet.Mines);
            Assert.AreEqual(100, planet.Factories);
        }

        [Test]
        public void TestInvadePlanetManyDefenders()
        {
            var planetOwner = new Player()
            {
                Num = 0
            };
            var fleetOwner = new Player()
            {
                Num = 1
            };

            var planet = new Planet()
            {
                Name = "Brin",
                PlayerNum = planetOwner.Num,
                Population = 10000,
                Mines = 100,
                Factories = 100,
                Defenses = 0,
            };

            // one mini-bomber
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Teamster #1",
            };

            fleet.Orbiting = planet;

            // 5000 attackers for 10000 undefended defenders, defenders win
            int attackers = 5000;
            invasionProcessor.InvadePlanet(planet, planetOwner, fleetOwner, fleet, attackers);

            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
            Assert.AreEqual(4500, planet.Population);
            Assert.AreEqual(planetOwner.Num, planet.PlayerNum);

            Assert.AreEqual(100, planet.Mines);
            Assert.AreEqual(100, planet.Factories);
        }

        [Test]
        public void TestInvadePlanetWithDefenses()
        {
            var planetOwner = new Player()
            {
                Num = 0
            };
            var fleetOwner = new Player()
            {
                Num = 1
            };

            var planet = new Planet()
            {
                Name = "Brin",
                PlayerNum = planetOwner.Num,
                Population = 100000,
                Mines = 100,
                Factories = 100,
                Defenses = 1000,
            };

            // one mini-bomber
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Teamster #1",
            };

            fleet.Orbiting = planet;
            planet.Spec = planetService.ComputePlanetSpec(planet, planetOwner);

            // 100,000 attackers for 100,000 well defended defenders, defenders win
            int attackers = 100000;
            invasionProcessor.InvadePlanet(planet, planetOwner, fleetOwner, fleet, attackers);

            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
            Assert.AreEqual(planetOwner.Num, planet.PlayerNum);
            Assert.AreEqual(42000, planet.Population);

            Assert.AreEqual(100, planet.Mines);
            Assert.AreEqual(100, planet.Factories);
        }
    }
}