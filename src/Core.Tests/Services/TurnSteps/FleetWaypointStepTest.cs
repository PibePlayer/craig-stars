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
        PlayerService playerService;
        PlanetService planetService;
        InvasionProcessor invasionProcessor;
        PlanetDiscoverer planetDiscover;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            invasionProcessor = TestUtils.TestContainer.GetInstance<InvasionProcessor>();
            planetDiscover = TestUtils.TestContainer.GetInstance<PlanetDiscoverer>();
        }

        [Test]
        public void TestProcessLoadAll()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to load all on the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAmount, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
            step.Process();

            // should load 25 onto the fleet, leaving 75 on the planet
            Assert.AreEqual(75, planet.Cargo.Ironium);
            Assert.AreEqual(25, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAmount, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetAmountTo, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to unload all onto the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAmount, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(25, planet.Cargo.Ironium);
            Assert.AreEqual(75, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAmount, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(20);
            planet.Cargo = new Cargo();

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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
            gameRunner.ComputeAggregates(recompute: true);

            // configure this fleet to unload some of its cargo
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetWaypointTo, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo();

            var step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
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

            step = new FleetWaypoint0Step(gameRunner.GameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscover);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(20, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);
            Assert.IsFalse(fleet.Waypoints[0].TaskComplete);

        }


    }
}