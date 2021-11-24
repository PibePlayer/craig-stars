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
    public class FleetWaypointStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetWaypointStepTest));

        IRulesProvider rulesProvider;
        PlanetService planetService;
        InvasionProcessor invasionProcessor;
        PlanetDiscoverer planetDiscover;
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            invasionProcessor = TestUtils.TestContainer.GetInstance<InvasionProcessor>();
            planetDiscover = TestUtils.TestContainer.GetInstance<PlanetDiscoverer>();
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }

        [Test]
        public void TestProcessLoadAll()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load all on the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(100, fleet.Cargo.Ironium);
        }

        [Test]
        public void TestProcessLoadAmount()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAmount, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should load 25 onto the fleet, leaving 75 on the planet
            Assert.AreEqual(75, planet.Cargo.Ironium);
            Assert.AreEqual(25, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAmount, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should load 20 into the fleet leaving 0 on the planet
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);

        }

        [Test]
        public void TestProcessLoadSetAmountTo()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetAmountTo, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(75, planet.Cargo.Ironium);
            Assert.AreEqual(25, fleet.Cargo.Ironium);
            Assert.IsTrue(waypoint.TaskComplete);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetAmountTo, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should load 20 the ship, leaving 0 on the planet and not completing the task
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);
            Assert.IsFalse(waypoint.TaskComplete);

        }

        [Test]
        public void TestProcessUnloadAll()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload all onto the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            Assert.AreEqual(100, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);
        }

        [Test]
        public void TestProcessUnloadAmount()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAmount, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(25, planet.Cargo.Ironium);
            Assert.AreEqual(75, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAmount, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(20);
            planet.Cargo = new Cargo();

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(20, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);

        }

        [Test]
        public void TestProcessUnloadSetAmountTo()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetWaypointTo, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(25, planet.Cargo.Ironium);
            Assert.AreEqual(75, fleet.Cargo.Ironium);
            Assert.IsTrue(fleet.Waypoints[0].TaskComplete);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetWaypointTo, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(20);
            planet.Cargo = new Cargo();

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(20, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);
            Assert.IsFalse(fleet.Waypoints[0].TaskComplete);

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

            game.Planets.Add(planet);

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
            game.Fleets.Add(fleet);

            planet.OrbitingFleets.Add(fleet);
            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, task: WaypointTask.Colonize));

            gameRunner.ComputeSpecs(recompute: true);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
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

            game.Planets.Add(planet);

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
            game.Fleets.Add(fleet);

            planet.OrbitingFleets.Add(fleet);
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

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, planetService, invasionProcessor, planetDiscover, fleetSpecService);
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