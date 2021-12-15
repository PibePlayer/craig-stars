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
    public class FleetPatrolStepTest
    {
        [Test]
        public void TestPatrol()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var fleet1 = game.Fleets[0];
            var fleet2 = game.Fleets[1];

            // position fleet2 50 ly away
            fleet2.Position = new Vector2(30, 40);

            // make sure we know about this fleet
            FleetDiscoverer fleetDiscoverer = TestUtils.TestContainer.GetInstance<FleetDiscoverer>();
            fleetDiscoverer.Discover(player1, fleet2);
            // refresh the player guid maps
            player1.SetupMapObjectMappings();

            // make player1 enemies with player2
            player1.PlayerRelations[1].Relation = PlayerRelation.Enemy;
            fleet1.Waypoints[0] = Waypoint.PositionWaypoint(new Vector2(), task: WaypointTask.Patrol, patrolRange: 50);

            var step = new FleetPatrolStep(gameRunner.GameProvider, new TestRulesProvider(), TestUtils.TestContainer.GetInstance<FleetDiscoverer>());
            step.Process();

            // we should be set to attack
            Assert.AreEqual(2, fleet1.Waypoints.Count);
            Assert.AreEqual(fleet2, fleet1.Waypoints[1].Target);
            Assert.AreEqual(fleet2.Guid, player1.Fleets[0].Waypoints[1].TargetGuid);
        }

        [Test]
        public void TestPatrolTooFar()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var fleet1 = game.Fleets[0];
            var fleet2 = game.Fleets[1];

            // position fleet2 > 50 ly away
            fleet2.Position = new Vector2(50, 50);

            // make sure we know about this fleet
            FleetDiscoverer fleetDiscoverer = TestUtils.TestContainer.GetInstance<FleetDiscoverer>();
            fleetDiscoverer.Discover(player1, fleet2);
            // refresh the player guid maps
            player1.SetupMapObjectMappings();

            // make player1 enemies with player2
            player1.PlayerRelations[1].Relation = PlayerRelation.Enemy;
            fleet1.Waypoints[0] = Waypoint.PositionWaypoint(new Vector2(), task: WaypointTask.Patrol, patrolRange: 50);

            var step = new FleetPatrolStep(gameRunner.GameProvider, new TestRulesProvider(), TestUtils.TestContainer.GetInstance<FleetDiscoverer>());
            step.Process();

            // we should be set to attack
            Assert.AreEqual(1, fleet1.Waypoints.Count);
        }

        [Test]
        public void TestPatrolEnemiesOnly()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var fleet1 = game.Fleets[0];
            var fleet2 = game.Fleets[1];

            // position fleet2 50 ly away
            fleet2.Position = new Vector2(30, 40);

            // make sure we know about this fleet
            FleetDiscoverer fleetDiscoverer = TestUtils.TestContainer.GetInstance<FleetDiscoverer>();
            fleetDiscoverer.Discover(player1, fleet2);
            // refresh the player guid maps
            player1.SetupMapObjectMappings();

            // make player1 neutral with player2
            player1.PlayerRelations[1].Relation = PlayerRelation.Neutral;
            fleet1.Waypoints[0] = Waypoint.PositionWaypoint(new Vector2(), task: WaypointTask.Patrol, patrolRange: 50);

            var step = new FleetPatrolStep(gameRunner.GameProvider, new TestRulesProvider(), TestUtils.TestContainer.GetInstance<FleetDiscoverer>());
            step.Process();

            // we should be set to attack
            Assert.AreEqual(1, fleet1.Waypoints.Count);
        }
    }
}