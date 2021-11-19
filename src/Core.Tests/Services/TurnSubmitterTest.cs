using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using System.Linq;
using CraigStars.UniverseGeneration;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TurnSubmitterTest
    {
        IRulesProvider rulesProvider;
        FleetService fleetService;
        PlayerTechService playerTechService;
        PlayerService playerService;
        PlayerIntel playerIntel;
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
            playerTechService = TestUtils.TestContainer.GetInstance<PlayerTechService>();
            playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
            playerIntel = TestUtils.TestContainer.GetInstance<PlayerIntel>();
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }

        [Test]
        public void TestUpdateFleetActions()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            // make a second planet we can make a waypoint to
            game.Planets.Add(new Planet()
            {
                Name = "Planet 2",
                Position = new Vector2(100, 0),
                BaseHab = new Hab(),
                Hab = new Hab(),
                TerraformedAmount = new Hab(),
            });
            game.UpdateInternalDictionaries();

            // make our player aware of the planets and their fleets
            var playerPlanetReportGenerationStep = new PlayerPlanetReportGenerationStep(new Provider<Game>(game), playerIntel);
            playerPlanetReportGenerationStep.Process();
            player.SetupMapObjectMappings();

            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);

            scanStep.PreProcess(game.OwnedPlanets.ToList());
            scanStep.Process();

            var fleet = player.Fleets[0];

            var planet1 = fleet.Orbiting;
            var planet2 = player.AllPlanets.First(planet => !planet.Explored);
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet2));
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet1));

            player.SetupMapObjectMappings();

            var turnSubmitter = new TurnSubmitter(game, new FleetOrderExecutor(game, fleetService));
            turnSubmitter.UpdateFleetActions(player);

            var gameFleet = game.Fleets[0];
            Assert.AreEqual(3, gameFleet.Waypoints.Count);
            Assert.AreEqual(game.Planets[0], gameFleet.Waypoints[0].Target);
            Assert.AreEqual(game.Planets[1], gameFleet.Waypoints[1].Target);
            Assert.AreEqual(game.Planets[0], gameFleet.Waypoints[2].Target);

            // add a duplicate waypoint, we should still only have 3 waypoints
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet1));
            turnSubmitter.UpdateFleetActions(player);

            Assert.AreEqual(3, gameFleet.Waypoints.Count);
            Assert.AreEqual(game.Planets[0], gameFleet.Waypoints[0].Target);
            Assert.AreEqual(game.Planets[1], gameFleet.Waypoints[1].Target);
            Assert.AreEqual(game.Planets[0], gameFleet.Waypoints[2].Target);

        }

    }

}