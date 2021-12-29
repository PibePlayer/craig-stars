using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using FakeItEasy;
using Godot;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetMoveStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetMoveStepTest));

        IRulesProvider rulesProvider = new TestRulesProvider();
        PlayerService playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
        MineFieldDamager mineFieldDamager = TestUtils.TestContainer.GetInstance<MineFieldDamager>();
        ShipDesignDiscoverer designDiscoverer = TestUtils.TestContainer.GetInstance<ShipDesignDiscoverer>();
        FleetService fleetService = TestUtils.TestContainer.GetInstance<FleetService>();

        [Test]
        public void TestMove()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];

            // move along the x axis
            game.MoveMapObject(fleet, fleet.Position, new Vector2(0, 0));
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 0), warpFactor: 5));

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            step.Execute(new(), game.OwnedPlanets.ToList());

            // move at warp 5, use some fuel
            Assert.AreEqual(new Vector2(25, 0), fleet.Position);
            Assert.AreEqual(fleet.FuelCapacity - 4, fleet.Fuel);
        }

        /// <summary>
        /// Make sure we arrive at a planet when we complete a move
        /// </summary>
        [Test]
        public void TestMoveComplete()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];

            var planet = new Planet()
            {
                Name = "Destination",
                Position = new Vector2(10, 10),
            };
            planet.InitEmptyPlanet();
            game.AddMapObject(planet);

            // move towards a nearby planet
            game.MoveMapObject(fleet, fleet.Position, new Vector2(0, 0));
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, warpFactor: 6));

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            step.Execute(new(), game.OwnedPlanets.ToList());

            // move at warp 6 so we overshoot, use some fuel
            Assert.AreEqual(new Vector2(10, 10), fleet.Position);
            Assert.AreEqual(planet, fleet.Orbiting);
            Assert.AreEqual(1, fleet.Waypoints.Count);

            // make sure the fleet has one waypoint at this planet
            Assert.AreEqual(planet, fleet.Waypoints[0].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[0].Task);
            Assert.AreEqual(planet.Position, fleet.Waypoints[0].Position);

            // the fleet should have used some fuel
            Assert.AreEqual(fleet.FuelCapacity - 2, fleet.Fuel);
        }

        /// <summary>
        /// Make sure we arrive at a planet when we complete a move targetting a fleet orbiting a planet
        /// </summary>
        [Test]
        public void TestMoveCompleteTargetFleetOrbitingPlanet()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            var planet = new Planet()
            {
                Name = "Destination",
                Position = new Vector2(10, 10),
            };
            planet.InitEmptyPlanet();
            game.AddMapObject(planet);

            var fleet2 = new Fleet()
            {
                PlayerNum = player.Num,
                Orbiting = planet,
                Position = planet.Position,
                Waypoints = new List<Waypoint>() {
                    Waypoint.TargetWaypoint(planet)
                }
            };

            // move towards this fleet
            game.MoveMapObject(fleet, fleet.Position, new Vector2(0, 0));
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(fleet2, warpFactor: 6));

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            step.Execute(new(), game.OwnedPlanets.ToList());

            // move at warp 6 so we overshoot, use some fuel
            Assert.AreEqual(new Vector2(10, 10), fleet.Position);
            Assert.AreEqual(planet, fleet.Orbiting);
            Assert.AreEqual(1, fleet.Waypoints.Count);

            // make sure the fleet has one waypoint at this planet
            Assert.AreEqual(fleet2, fleet.Waypoints[0].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[0].Task);
            Assert.AreEqual(planet.Position, fleet.Waypoints[0].Position);

            // the fleet should have used some fuel
            Assert.AreEqual(fleet.FuelCapacity - 2, fleet.Fuel);
        }

        [Test]
        public void TestMoveCompleteRepeatOrders()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var sourcePlanet = game.Planets[0];

            var destPlanet = new Planet()
            {
                Name = "Destination",
                Position = new Vector2(10, 10),
            };
            destPlanet.InitEmptyPlanet();
            game.AddMapObject(destPlanet);

            // start us at the source planet
            game.MoveMapObject(fleet, fleet.Position, sourcePlanet.Position);
            fleet.Orbiting = sourcePlanet;

            // give directions to bounce back and forth and repeat
            fleet.Waypoints = new List<Waypoint>() {
                Waypoint.TargetWaypoint(sourcePlanet, warpFactor: 6),
                Waypoint.TargetWaypoint(destPlanet, warpFactor: 6),
            };
            fleet.RepeatOrders = true;

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);
            step.Execute(new(), game.OwnedPlanets.ToList());

            // move to the destPlanet
            Assert.AreEqual(destPlanet.Position, fleet.Position);
            Assert.AreEqual(destPlanet, fleet.Orbiting);

            // we should still have 2 waypoints because of repeating orders
            Assert.AreEqual(2, fleet.Waypoints.Count);

            // make sure the fleet is targeting the destPlanet for waypoint0
            Assert.AreEqual(destPlanet, fleet.Waypoints[0].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[0].Task);
            Assert.AreEqual(destPlanet.Position, fleet.Waypoints[0].Position);

            // make sure the fleet is targeting the original sourcePlanet for waypoint1
            Assert.AreEqual(sourcePlanet, fleet.Waypoints[1].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[1].Task);
            Assert.AreEqual(sourcePlanet.Position, fleet.Waypoints[1].Position);
        }

        [Test]
        public void TestMovePartialRepeatOrders()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var sourcePlanet = game.Planets[0];

            // create a planet that is 2 years away at warp 5
            var destPlanet = new Planet()
            {
                Name = "Destination",
                Position = new Vector2(0, 26),
            };
            destPlanet.InitEmptyPlanet();
            game.AddMapObject(destPlanet);

            // start us at the source planet
            game.MoveMapObject(fleet, fleet.Position, sourcePlanet.Position);
            fleet.Orbiting = sourcePlanet;

            // give directions to bounce back and forth and repeat
            fleet.Waypoints = new List<Waypoint>() {
                Waypoint.TargetWaypoint(sourcePlanet, warpFactor: 5),
                Waypoint.TargetWaypoint(destPlanet, warpFactor: 5),
            };
            fleet.RepeatOrders = true;

            // run one execution
            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);
            step.Execute(new(), game.OwnedPlanets.ToList());

            // move towards the destPlanet, away from the sourcePlanet
            Assert.AreEqual(new Vector2(0, 25), fleet.Position);
            Assert.AreEqual(null, fleet.Orbiting);
            Assert.AreEqual(null, fleet.Waypoints[0].Target);
            Assert.AreEqual(sourcePlanet, fleet.Waypoints[0].OriginalTarget);
            Assert.AreEqual(sourcePlanet.Position, fleet.Waypoints[0].OriginalPosition);

            // make sure the fleet is targeting the destPlanet for waypoint1
            Assert.AreEqual(destPlanet, fleet.Waypoints[1].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[1].Task);
            Assert.AreEqual(destPlanet.Position, fleet.Waypoints[1].Position);

            // run the second execution
            step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);
            step.Execute(new(), game.OwnedPlanets.ToList());

            // we should have arrived at the destPlanet
            Assert.AreEqual(destPlanet.Position, fleet.Position);
            Assert.AreEqual(destPlanet, fleet.Orbiting);

            // we should still have 2 waypoints because of repeating orders
            Assert.AreEqual(2, fleet.Waypoints.Count);

            // make sure the fleet is targeting the destPlanet for waypoint0
            Assert.AreEqual(destPlanet, fleet.Waypoints[0].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[0].Task);
            Assert.AreEqual(destPlanet.Position, fleet.Waypoints[0].Position);

            // make sure the fleet is targeting the original sourcePlanet for waypoint1
            Assert.AreEqual(sourcePlanet, fleet.Waypoints[1].Target);
            Assert.AreEqual(WaypointTask.None, fleet.Waypoints[1].Task);
            Assert.AreEqual(sourcePlanet.Position, fleet.Waypoints[1].Position);
        }

        [Test]
        public void TestMoveEngineFailure()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var player = game.Players[0];

            player.Race.LRTs.Add(LRT.CE);
            gameRunner.ComputeSpecs(true);

            // make sure our random number generator returns "yes, fail engines"
            IRulesProvider mockRulesProvider = A.Fake<IRulesProvider>();
            var mockRules = A.Fake<Rules>();
            var random = A.Fake<Random>();
            A.CallTo(() => mockRulesProvider.Rules).Returns(mockRules);
            mockRules.Random = random;

            // move along the x axis
            game.MoveMapObject(fleet, fleet.Position, new Vector2(0, 0));
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 0), warpFactor: 7));

            // make the engine fail
            A.CallTo(() => random.NextDouble()).Returns(.1);
            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, mockRulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            step.Execute(new(), game.OwnedPlanets.ToList());

            // move at warp 5, shouldn't move at all
            Assert.AreEqual(new Vector2(0, 0), fleet.Position);
            Assert.AreEqual(MessageType.FleetEngineFailure, player.Messages[0].Type);
        }

        [Test]
        public void TestCheckForMineFieldHitSpeed()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // test a 10 ly radius field at 0, 0
            var radius = 10;

            game.AddMapObject(new MineField()
            {
                PlayerNum = player2.Num,
                Type = MineFieldType.SpeedBump,
                Position = new Vector2(0, 0),
                NumMines = radius * radius,
            });

            // make a new fleet at -15x, and move it through the field
            var design = ShipDesigns.LongRangeScount.Clone();
            design.PlayerNum = player1.Num;
            game.Designs.Add(design);
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
                Position = new Vector2(-15, 0)
            };
            game.AddMapObject(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

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

            game.AddMapObject(new MineField()
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
            gameRunner.ComputeSpecs(recompute: true);

            // make the normal minefield allow speed 5, 25% hit chance per warp
            // we'll go warp 9 to guarantee a hit (if we were to fly through it)
            game.Rules.MineFieldStatsByType[MineFieldType.Standard].MaxSpeed = 5;
            game.Rules.MineFieldStatsByType[MineFieldType.Standard].ChanceOfHit = .25f;

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            // send the fleet at warp 9, straight up, should miss minefield
            var dest = new Waypoint() { Position = new Vector2(-15, 20), WarpFactor = 9 };
            var dist = dest.WarpFactor * dest.WarpFactor;
            var actualDist = step.CheckForMineFields(fleet, player1, dest, dist);

            // we should go our normal distance
            Assert.AreEqual(dist, actualDist, .01);
            Assert.AreEqual(0, fleet.Tokens[0].Damage);
            Assert.AreEqual(0, fleet.Tokens[0].QuantityDamaged);

            // send the fleet at warp 5, through the minefield, should fly without hitting
            game.MoveMapObject(fleet, fleet.Position, new Vector2(-10, 0));
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