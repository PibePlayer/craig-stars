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
    public class FleetTransferStepTest
    {

        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }

        [Test]
        public void TestTransferFleet()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var fleet1 = game.Fleets[0];
            var fleet2 = game.Fleets[1];

            fleet1.Waypoints[0] = Waypoint.PositionWaypoint(new Vector2(), task: WaypointTask.TransferFleet, transferToPlayer: player2.Num);

            var step = new FleetTransferStep(gameRunner.GameProvider, new TestRulesProvider(), fleetSpecService);
            step.Process();

            // we should have the same number of fleets, but fleet1 should belong to player2 biw,
            Assert.AreEqual(2, game.Fleets.Count);
            Assert.AreEqual(player2.Num, fleet1.PlayerNum);
        }

    }
}