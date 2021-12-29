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
    public class FleetColonizeStepTest
    {

        IRulesProvider rulesProvider;
        PlanetService planetService;
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }

        [Test]
        public void TestColonize()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            var planet = new Planet()
            {
                Name = "Planet To Colonize",
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
                Cargo = new Cargo(),
            };

            game.AddMapObject(planet);

            // build a colony fleet
            var colonizer = TestUtils.CreateDesign(game, player, ShipDesigns.SantaMaria);
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(colonizer, 1)
                },
                Cargo = new Cargo(colonists: 25)
            };
            game.AddMapObject(fleet);

            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, task: WaypointTask.Colonize));

            gameRunner.ComputeSpecs(recompute: true);

            var step = new FleetColonize0Step(gameRunner.GameProvider, rulesProvider, planetService, fleetSpecService);
            step.Process();

            // we should colonize the planet and have some scrap resources
            Assert.AreEqual(player.Num, planet.PlayerNum);
            Assert.AreEqual(2500, planet.Population);
            Assert.Greater(planet.Cargo.Total, 25);
        }

        [Test]
        public void TestColonizeAR()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            var planet = new Planet()
            {
                Name = "Planet To Colonize",
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
                Cargo = new Cargo(),
            };

            game.AddMapObject(planet);

            // build a colony fleet with an orbital construction module
            var colonizer = TestUtils.CreateDesign(game, player, ShipDesigns.SantaMaria);
            colonizer.Slots[1].HullComponent = Techs.OrbitalConstructionModule;
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(colonizer, 1)
                },
                Cargo = new Cargo(colonists: 25)
            };
            game.AddMapObject(fleet);

            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, task: WaypointTask.Colonize));

            // add a starter colony
            var starterColony = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Starter Colony",
                Purpose = ShipDesignPurpose.StarterColony,
                Hull = Techs.OrbitalFort,
                HullSetNumber = player.DefaultHullSet,
                CanDelete = false,
            };
            game.Designs.Add(starterColony);

            gameRunner.ComputeSpecs(recompute: true);

            var step = new FleetColonize0Step(gameRunner.GameProvider, rulesProvider, planetService, fleetSpecService);
            step.Process();

            // we should colonize the planet and have some scrap resources
            Assert.AreEqual(player.Num, planet.PlayerNum);
            Assert.AreEqual(2500, planet.Population);
            Assert.Greater(planet.Cargo.Total, 25);

            // we should have an orbital fort
            Assert.IsTrue(planet.HasStarbase);
            Assert.AreEqual(starterColony, planet.Starbase.Tokens[0].Design);
        }
    }
}