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
    public class FleetLoadStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetLoadStepTest));

        IRulesProvider rulesProvider;
        CargoTransferer cargoTransferer;
        InvasionProcessor invasionProcessor;
        FleetService fleetService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            cargoTransferer = TestUtils.TestContainer.GetInstance<CargoTransferer>();
            invasionProcessor = TestUtils.TestContainer.GetInstance<InvasionProcessor>();
            fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
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
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100, colonists: 10);

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(100, fleet.Cargo.Ironium);
            Assert.AreEqual(0, planet.Cargo.Colonists);
            Assert.AreEqual(10, fleet.Cargo.Colonists);
            Assert.AreEqual($"{fleet.Name} has loaded 100 Ironium from {planet.Name}", player.Messages[0].Text);
            Assert.AreEqual($"{fleet.Name} has beamed 1000 Colonists from {planet.Name}", player.Messages[1].Text);
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

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should load 25 onto the fleet, leaving 75 on the planet
            Assert.AreEqual(75, planet.Cargo.Ironium);
            Assert.AreEqual(25, fleet.Cargo.Ironium);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAmount, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should load 20 into the fleet leaving 0 on the planet
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);

        }

        [Test]
        public void TestProcessLoadOptimal()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planetTarget = game.Planets[0];
            var fleetTarget = game.Fleets[0];
            var player = game.Players[0];

            // make a new fleet orbiting the planet
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Position = planetTarget.Position,
                Orbiting = planetTarget,
                Tokens = new List<ShipToken>() {
                    new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1),
                },
                Waypoints = new List<Waypoint>()
                {
                    Waypoint.TargetWaypoint(planetTarget)
                },

            };
            game.AddMapObject(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load optimal fuel to the target
            var waypoint = fleet.Waypoints[0];
            waypoint.Target = planetTarget;
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal));
            fleet.Spec.FuelCapacity = 200;
            fleet.Fuel = 200;

            // set another waypoint 25 l.y. away so we have some estimated fuel usage
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(25, 0)));

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // we are set to load optimal, but we are at a planet, so no need to move any fuel around
            Assert.AreEqual(fleetTarget.Fuel, fleetTarget.FuelCapacity);

            // target a fleet instead
            waypoint.Target = fleetTarget;
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal));
            fleet.Spec.FuelCapacity = 200;
            fleet.Fuel = 200;
            fleetTarget.Fuel = 0;
            fleetTarget.Spec.FuelCapacity = 100;

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // we are set to load optimal and we are only using 17mg of fuel for the next trip, give up 100mg of fuel
            Assert.AreEqual(100, fleetTarget.Fuel);
            Assert.AreEqual(100, fleet.Fuel);

            // set our fuel to 100 and the target to 0
            // we should transfer all we can, but leave enough for our next trip (17mg)
            fleet.Fuel = 100;
            fleetTarget.Fuel = 0;

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // we are set to load optimal and we are only using 17mg of fuel for the next trip, give up 100mg of fuel
            Assert.AreEqual(17, fleet.Fuel);
            Assert.AreEqual(100 - 17, fleetTarget.Fuel);
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

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(75, planet.Cargo.Ironium);
            Assert.AreEqual(25, fleet.Cargo.Ironium);
            Assert.IsFalse(waypoint.WaitAtWaypoint);

            // try to unload more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.SetAmountTo, 25));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should load 20 the ship, leaving 0 on the planet and not completing the task
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);
            Assert.IsTrue(waypoint.WaitAtWaypoint);

        }

        [Test]
        public void TestProcessLoadFillPercentWaitPercent()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];
            var player = game.Players[0];

            // create a cargo freighter with 210kT
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load 10% of 210kT, or 21kT
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.FillPercent, 10));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100);

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should unload 25 onto the planet, leaving 75 on the ship
            Assert.AreEqual(79, planet.Cargo.Ironium);
            Assert.AreEqual(21, fleet.Cargo.Ironium);
            Assert.IsFalse(waypoint.WaitAtWaypoint);

            // try to fill more than we have
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.FillPercent, 100));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should load 20 the ship, leaving 0 on the planet, but moving on
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);
            Assert.IsFalse(waypoint.WaitAtWaypoint);

            // try to fill more than we have, but use WaitForPercent
            waypoint.Task = WaypointTask.Transport;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.WaitForPercent, 100));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 20);

            step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // should load 20 the ship, leaving 0 on the planet, and we should wait
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(20, fleet.Cargo.Ironium);
            Assert.IsTrue(waypoint.WaitAtWaypoint);
        }

        [Test]
        public void TestProcessLoadAllForeign()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var planet = game.Planets[1];
            var fleet = game.Fleets[0];
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player1, ShipDesigns.Teamster.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load all on the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.Target = planet;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100, colonists: 10);

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // did not work
            Assert.AreEqual(100, planet.Cargo.Ironium);
            Assert.AreEqual(0, fleet.Cargo.Ironium);
            Assert.AreEqual(10, planet.Cargo.Colonists);
            Assert.AreEqual(0, fleet.Cargo.Colonists);
        }

        [Test]
        public void TestProcessLoadAllForeignPlanetSS()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var planet = game.Planets[1];
            var fleet = game.Fleets[0];
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            fleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player1, ShipDesigns.RobberBaroner.Clone(game.Players[0])), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load all on the planet
            var waypoint = fleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.Target = planet;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100, colonists: 10);

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // stole ironium
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(100, fleet.Cargo.Ironium);

            // can't steal people
            Assert.AreEqual(10, planet.Cargo.Colonists);
            Assert.AreEqual(0, fleet.Cargo.Colonists);
        }

        [Test]
        public void TestProcessLoadAllForeignFleetSS()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var stealerFleet = game.Fleets[0];
            var markFleet = game.Fleets[1];
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            stealerFleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player1, ShipDesigns.RobberBaroner.Clone(player1)), 1));
            markFleet.Tokens.Add(new ShipToken(TestUtils.CreateDesign(game, player1, ShipDesigns.Teamster.Clone(player2)), 1));
            gameRunner.ComputeSpecs(recompute: true);

            // configure this fleet to load all on the planet
            var waypoint = stealerFleet.Waypoints[0];
            waypoint.Task = WaypointTask.Transport;
            waypoint.Target = markFleet;
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll));
            stealerFleet.Cargo = new Cargo();
            markFleet.Cargo = new Cargo(ironium: 100, colonists: 10);

            var step = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, cargoTransferer, invasionProcessor, fleetService);
            step.Process();

            // stole ironium
            Assert.AreEqual(0, markFleet.Cargo.Ironium);
            Assert.AreEqual(100, stealerFleet.Cargo.Ironium);

            // can't steal people
            Assert.AreEqual(10, markFleet.Cargo.Colonists);
            Assert.AreEqual(0, stealerFleet.Cargo.Colonists);
        }        
    }
}