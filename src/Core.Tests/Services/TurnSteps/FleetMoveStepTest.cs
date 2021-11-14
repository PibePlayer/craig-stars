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
    public class FleetMoveStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetMoveStepTest));

        PlayerService playerService = new PlayerService(new TestRulesProvider());
        MineFieldDamager mineFieldDamager = new();
        ShipDesignDiscoverer designDiscoverer = new();
        FleetService fleetService = new();

        [Test]
        public void TestCheckForMineFieldHitSpeed()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // test a 10 ly radius field at 0, 0
            var radius = 10;

            game.MineFields.Add(new MineField()
            {
                PlayerNum = player2.Num,
                Type = MineFieldType.SpeedBump,
                Position = new Vector2(0, 0),
                NumMines = radius * radius,
            });

            // make a new fleet at -15x, and move it through the field
            var design = ShipDesigns.LongRangeScount.Clone();
            design.PlayerNum = player1.Num;
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
                Position = new Vector2(-15, 0)
            };
            fleet.ComputeAggregate(player1);

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            // make the speed minefield allow speed 5, 25% hit chance per warp
            // we'll go warp 9 to guarantee a hit
            game.Rules.MineFieldStatsByType[MineFieldType.SpeedBump].MaxSpeed = 5;
            game.Rules.MineFieldStatsByType[MineFieldType.SpeedBump].ChanceOfHit = .25f;

            // send the fleet at warp 9, straight through the minefield
            var dest = new Waypoint() { Position = new Vector2(20, 0), WarpFactor = 9 };
            var dist = dest.WarpFactor * dest.WarpFactor;
            var actualDist = step.CheckForMineFields(fleet, player1, dest, dist);

            // we should come to a dead stop
            Assert.AreEqual(5, actualDist, .01);
            Assert.AreEqual(0, fleet.Tokens[0].Damage);
            Assert.AreEqual(0, fleet.Tokens[0].QuantityDamaged);

            // change the rules to a 10% chance of hitting and make sure we still collide (though further through)
            game.Rules.MineFieldStatsByType[MineFieldType.SpeedBump].ChanceOfHit = .1f;

            dest = new Waypoint() { Position = new Vector2(20, 0), WarpFactor = 9 };
            dist = dest.WarpFactor * dest.WarpFactor;
            actualDist = step.CheckForMineFields(fleet, player1, dest, dist);

            // we should come to a dead stop
            Assert.Greater(actualDist, 5);
            Assert.Less(actualDist, dist);
            Assert.AreEqual(0, fleet.Tokens[0].Damage);
            Assert.AreEqual(0, fleet.Tokens[0].QuantityDamaged);

        }

        [Test]
        public void TestCheckForMineFieldMiss()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // test a 10 ly radius field at 0, 0
            var radius = 10;

            game.MineFields.Add(new MineField()
            {
                PlayerNum = player2.Num,
                Position = new Vector2(0, 0),
                NumMines = radius * radius,
            });

            // make a new fleet at -15x, and move it through the field
            var design = ShipDesigns.LongRangeScount.Clone();
            design.PlayerNum = player1.Num;
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
                Position = new Vector2(-15, 0)
            };
            fleet.ComputeAggregate(player1);

            // make the normal minefield allow speed 5, 25% hit chance per warp
            // we'll go warp 9 to guarantee a hit (if we were to fly through it)
            game.Rules.MineFieldStatsByType[MineFieldType.Standard].MaxSpeed = 5;
            game.Rules.MineFieldStatsByType[MineFieldType.Standard].ChanceOfHit = .25f;

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            // send the fleet at warp 9, straight up, should miss minefield
            var dest = new Waypoint() { Position = new Vector2(-15, 20), WarpFactor = 9 };
            var dist = dest.WarpFactor * dest.WarpFactor;
            var actualDist = step.CheckForMineFields(fleet, player1, dest, dist);

            // we should go our normal distance
            Assert.AreEqual(dist, actualDist, .01);
            Assert.AreEqual(0, fleet.Tokens[0].Damage);
            Assert.AreEqual(0, fleet.Tokens[0].QuantityDamaged);

            // send the fleet at warp 5, through the minefield, should fly without hitting
            fleet.Position = new Vector2(-10, 0);
            dest = new Waypoint() { Position = new Vector2(15, 0), WarpFactor = 5 };
            dist = dest.WarpFactor * dest.WarpFactor;
            actualDist = step.CheckForMineFields(fleet, player1, dest, dist);

            // we should go through
            Assert.AreEqual(dist, actualDist, .01);
            Assert.AreEqual(0, fleet.Tokens[0].Damage);
            Assert.AreEqual(0, fleet.Tokens[0].QuantityDamaged);
        }

    }
}