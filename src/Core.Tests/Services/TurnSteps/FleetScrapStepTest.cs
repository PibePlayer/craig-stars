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
    public class FleetScrapStepTest
    {
        /// <summary>
        /// Scrap a fleet over a planet, make sure we get minerals
        /// </summary>
        [Test]
        public void TestScrapFleetOverPlanet()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];

            fleet.Waypoints[0] = Waypoint.TargetWaypoint(planet, task: WaypointTask.ScrapFleet);

            var step = new FleetScrapStep(gameRunner.GameProvider, new TestRulesProvider());
            step.Execute(new(), game.OwnedPlanets.ToList());

            gameRunner.OnPurgeDeletedMapObjects();

            // we should have the same number of fleets, but fleet1 should belong to player2 biw,
            Assert.AreEqual(0, game.Fleets.Count);
            Assert.AreEqual(new Cargo(6, 0, 2).ToMineral(), planet.Cargo.ToMineral());
        }

        /// <summary>
        /// Scrap a fleet over a planet, make sure we get minerals
        /// </summary>
        [Test]
        public void TestScrapFleetInSpace()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            // move teh fleet into deep space
            game.MoveMapObject(fleet, fleet.Position, new Vector2(10, 10));
            fleet.Orbiting = null;

            fleet.Waypoints[0] = Waypoint.PositionWaypoint(fleet.Position, task: WaypointTask.ScrapFleet);

            var step = new FleetScrapStep(gameRunner.GameProvider, new TestRulesProvider());
            step.Execute(new(), game.OwnedPlanets.ToList());

            gameRunner.OnPurgeDeletedMapObjects();

            // we should have the same number of fleets, but fleet1 should belong to player2 biw,
            Assert.AreEqual(0, game.Fleets.Count);

            // we should create some salvage
            Assert.AreEqual(1, game.Salvage.Count);
            Assert.AreEqual(new Cargo(6, 0, 2), game.Salvage[0].Cargo);
            Assert.AreEqual(player.Num, game.Salvage[0].PlayerNum);
        }

    }
}