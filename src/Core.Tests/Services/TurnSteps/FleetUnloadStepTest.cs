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
    public class FleetUnloadStepTest
    {
        IRulesProvider rulesProvider;
        CargoTransferer cargoTransferer;
        InvasionProcessor invasionProcessor;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            cargoTransferer = TestUtils.TestContainer.GetInstance<CargoTransferer>();
            invasionProcessor = TestUtils.TestContainer.GetInstance<InvasionProcessor>();
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
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll));
            fleet.Cargo = fleet.Cargo.WithIronium(100).WithColonists(10);
            planet.Cargo = new Cargo();

            var step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
            step.Process();

            Assert.AreEqual(100, planet.Cargo.Ironium);
            Assert.AreEqual(10, planet.Cargo.Colonists);
            Assert.AreEqual(new Cargo(), fleet.Cargo);
            Assert.AreEqual($"{fleet.Name} has unloaded 100 Ironium to {planet.Name}", player.Messages[0].Text);
            Assert.AreEqual($"{fleet.Name} has beamed 1000 Colonists to {planet.Name}", player.Messages[1].Text);
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

            var step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(25, planet.Cargo.Ironium);
            Assert.AreEqual(75, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAmount, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(20);
            planet.Cargo = new Cargo();

            step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
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

            var step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(25, planet.Cargo.Ironium);
            Assert.AreEqual(75, fleet.Cargo.Ironium);
            Assert.IsFalse(fleet.Waypoints[0].WaitAtWaypoint);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetWaypointTo, 25));
            fleet.Cargo = fleet.Cargo.WithIronium(20);
            planet.Cargo = new Cargo();

            step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
            step.Process();

            // should unload 20 onto the planet, leaving 0 on the ship and the fleet should wait
            Assert.AreEqual(20, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);
            Assert.IsTrue(fleet.Waypoints[0].WaitAtWaypoint);
        }

        [Test]
        public void TestProcessUnloadAllForeignPlanet()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var planet = game.Planets[1];
            var fleet = game.Fleets[0];
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player1, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to unload all onto the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.Target = planet;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll));
            fleet.Cargo = fleet.Cargo.WithIronium(100);
            planet.Cargo = new Cargo(colonists: 250);

            var step = new FleetUnload0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor);
            step.Process();

            Assert.AreEqual(100, planet.Cargo.Ironium);
            Assert.AreEqual(250, planet.Cargo.Colonists);
            Assert.AreEqual($"{fleet.Name} has unloaded 100 Ironium to {planet.Name}", player1.Messages[0].Text);
        }
    }
}