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
    public class FleetLoadDunnageStepTest
    {
        IRulesProvider rulesProvider;
        InvasionProcessor invasionProcessor;
        FleetService fleetService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            invasionProcessor = TestUtils.TestContainer.GetInstance<InvasionProcessor>();
            fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
        }

        [Test]
        public void TestProcessLoadDunnage()
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
            waypoint.TransportTasks = new WaypointTransportTasks(ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll), colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadDunnage));
            fleet.Cargo = new Cargo();
            planet.Cargo = new Cargo(ironium: 100, colonists: 220);

            // dunnage takes place after load, so for a good test we'll do a load and then a dunnage load
            var loadStep = new FleetLoad0Step(gameRunner.GameProvider, rulesProvider, invasionProcessor, fleetService);
            var step = new FleetLoadDunnage0Step(gameRunner.GameProvider, rulesProvider, invasionProcessor);
            loadStep.Process();
            step.Process();

            // should load all available colonists into available cargo space
            Assert.AreEqual(0, planet.Cargo.Ironium);
            Assert.AreEqual(110, planet.Cargo.Colonists);
            Assert.AreEqual(100, fleet.Cargo.Ironium);
            Assert.AreEqual(110, fleet.Cargo.Colonists);
            Assert.AreEqual($"{fleet.Name} has loaded 100 Ironium from {planet.Name}", player.Messages[0].Text);
            Assert.AreEqual($"{fleet.Name} has beamed 11000 Colonists from {planet.Name}", player.Messages[1].Text);
        }

    }
}