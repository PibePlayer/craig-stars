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
    public class FleetMergeStepTest
    {

        IRulesProvider rulesProvider;
        FleetService fleetService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
        }

        [Test]
        public void TestMergeWithFleet()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            // build a colony fleet
            var colonizer = TestUtils.CreateDesign(game, player, ShipDesigns.SantaMaria);
            var mergeTarget = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(colonizer, 1)
                },
                Cargo = new Cargo(colonists: 25)
            };
            game.AddMapObject(mergeTarget);

            fleet.Waypoints[0] = Waypoint.TargetWaypoint(mergeTarget, task: WaypointTask.MergeWithFleet);

            gameRunner.ComputeSpecs(recompute: true);

            var step = new FleetMerge0Step(gameRunner.GameProvider, rulesProvider, fleetService);
            step.Process();

            // make sure our fleet goes away
            gameRunner.OnPurgeDeletedMapObjects();

            // we should end up with a single fleet with 2 tokens after the merge
            Assert.AreEqual(1, game.Fleets.Count);
            Assert.AreEqual(mergeTarget, game.Fleets[0]);
            Assert.AreEqual(2, mergeTarget.Tokens.Count);
        }

    }
}