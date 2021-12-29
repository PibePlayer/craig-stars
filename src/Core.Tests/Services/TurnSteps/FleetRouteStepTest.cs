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
    public class FleetRouteStepTest
    {

        IRulesProvider rulesProvider;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
        }

        [Test]
        public void TestRoute()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var routePlanet = game.Planets[0];
            var fleet = game.Fleets[0];

            var routeTargetPlanet = new Planet();
            routeTargetPlanet.InitEmptyPlanet();
            game.AddMapObject(routeTargetPlanet);

            routePlanet.RouteTarget = routeTargetPlanet;

            fleet.Waypoints.Add(Waypoint.TargetWaypoint(routeTargetPlanet, task: WaypointTask.Route));

            var step = new FleetRoute0Step(gameRunner.GameProvider, rulesProvider);
            step.Process();

            // we should be routed to the route target
            Assert.AreEqual(2, fleet.Waypoints.Count);
            Assert.AreEqual(routeTargetPlanet, fleet.Waypoints[1].Target);
        }

    }
}