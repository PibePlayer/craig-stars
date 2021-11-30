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
using CraigStars.UniverseGeneration;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerScanStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerScanStepTest));

        TestRulesProvider rulesProvider;
        PlayerTechService playerTechService;
        PlayerIntel playerIntel;
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            playerTechService = TestUtils.TestContainer.GetInstance<PlayerTechService>();
            playerIntel = TestUtils.TestContainer.GetInstance<PlayerIntel>();
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }


        [Test]
        public void TestScan()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            game.Planets[0].Population = 120000;

            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);
            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // our player should know about the planet updates
            Assert.AreEqual(game.Planets[0].Population, game.Players[0].Planets[0].Population);
            Assert.IsTrue(game.Players[0].Fleets.Count > 0);
        }

        [Test]
        public void TestScanPlanets()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            player.TechLevels.Electronics = 3;
            gameRunner.ComputeSpecs(recompute: true);

            // our fleet should have a penscan range of
            var scanRangePen = fleet.Spec.ScanRangePen;

            var planet2 = new Planet()
            {
                Name = "Planet 2",
                // move this planet just out of range along the x axis
                Position = fleet.Position + new Vector2(scanRangePen - 1, 0),
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(1, 2, 3),
                Hab = new Hab(1, 2, 3),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(4, 5, 6),
            };

            game.Planets.Add(planet2);

            // discover the basics about this planet
            playerIntel.Discover(player, planet2);

            // we shouldn't know the hab yet
            var playerPlanet = player.ForeignPlanets[1];
            Assert.IsNull(playerPlanet.Hab);

            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);
            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // we should know the hab now
            Assert.AreEqual(planet2.Hab, playerPlanet.Hab);
        }

        [Test]
        public void TestScanPlanetsWithMovingFleet()
        {
            // turn on this feature
            rulesProvider.TestRules.FleetsScanWhileMoving = true;

            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            player.TechLevels.Electronics = 3;
            gameRunner.ComputeSpecs(recompute: true);

            // our fleet should have a penscan range of
            var scanRangePen = fleet.Spec.ScanRangePen;

            var planet2 = new Planet()
            {
                Name = "Planet 2",
                // move this planet just out of range along the x axis
                Position = fleet.Position + new Vector2(scanRangePen + 1, 0),
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(1, 2, 3),
                Hab = new Hab(1, 2, 3),
                MineralConcentration = new Mineral(4, 5, 6),
            };

            game.Planets.Add(planet2);

            // discover the basics about this planet
            playerIntel.Discover(player, planet2);

            // we shouldn't know the hab yet
            var playerPlanet = player.ForeignPlanets[1];
            Assert.IsNull(playerPlanet.Hab);

            // simulate this fleet moving past the planet and out of scan range
            fleet.PreviousPosition = new Vector2();
            fleet.Position = new Vector2(scanRangePen * 2 + 2, 0);

            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);
            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // we should know the hab now
            Assert.AreEqual(planet2.Hab, playerPlanet.Hab);
        }

        [Test]
        public void TestScanFleets()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];

            // our scan won't work unless we know about our own designs
            foreach (var design in game.Designs.Where(d => d.PlayerNum == player1.Num))
            {
                playerIntel.Discover(player1, design);
            }

            var fleet2 = game.Fleets[1];

            // create a pen scanner on top of our other fleet (which is behind a planet)
            var scanners = new List<PlayerScanStep.Scanner>() {
                new PlayerScanStep.Scanner(fleet2.Position, rangePen: 1)
            };

            // we should discover this fleet
            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);
            scanStep.ScanFleets(player1, scanners);

            Assert.AreEqual(1, player1.ForeignFleets.Count);
            Assert.AreEqual(fleet2.Guid, player1.ForeignFleets[0].Guid);
            Assert.AreEqual(1, player1.ForeignDesigns.Count);
            Assert.AreEqual(fleet2.Tokens[0].Design.Guid, player1.ForeignDesigns[0].Guid);
        }

        // Disabled because this is long running
        // [Test]
        public void TestScanManyFleets()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // our scan won't work unless we know about our own designs
            playerIntel.Discover(player1, game.Designs[0]);

            // make many fleets
            player1.TechLevels = new TechLevel(electronics: 16);
            var planet = game.Planets[1];
            var random = game.Rules.Random;
            var scanners = new List<PlayerScanStep.Scanner>();
            for (int i = 0; i < 1000; i++)
            {
                var isOrbiting = random.Next() % 2 > 0;

                var position1 = isOrbiting ? planet.Position : new Vector2(random.Next(1000), random.Next(1000));
                var design1 = ShipDesigns.LongRangeScount.Clone(player1);
                design1.Name = "Design " + i;
                var fleet1 = new Fleet()
                {
                    PlayerNum = player1.Num,
                    Name = "New Fleet " + i,
                    Position = position1,
                    Tokens = new List<ShipToken>() {
                        new ShipToken(design1, 1),
                    },
                    BattlePlan = player1.BattlePlans[0],
                    Orbiting = isOrbiting ? planet : null,
                    Waypoints = new List<Waypoint>() {
                        isOrbiting ? Waypoint.TargetWaypoint(planet) : Waypoint.PositionWaypoint(position1)
                    }
                };
                game.Fleets.Add(fleet1);
                game.Designs.Add(design1);

                var position2 = isOrbiting ? planet.Position : new Vector2(random.Next(1000), random.Next(1000));
                var design2 = ShipDesigns.Teamster.Clone(player2);
                design2.Name = "Design " + i;
                var fleet2 = new Fleet()
                {
                    PlayerNum = player2.Num,
                    Name = "New Fleet " + i,
                    Position = position2,
                    Tokens = new List<ShipToken>() {
                        new ShipToken(design2, 1),
                    },
                    BattlePlan = player2.BattlePlans[0],
                    Orbiting = isOrbiting ? planet : null,
                    Waypoints = new List<Waypoint>() {
                        isOrbiting ? Waypoint.TargetWaypoint(planet) : Waypoint.PositionWaypoint(position2)
                    }
                };
                game.Fleets.Add(fleet2);
                game.Designs.Add(design2);


            }

            game.UpdateInternalDictionaries();
            gameRunner.ComputeSpecs();

            // we should discover this fleet
            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < 500; i++)
            {
                scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());
            }
            stopwatch.Stop();
            log.Debug($"Completed All Scans: ({TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds):c})");

        }

        [Test]
        public void TestDiscoverStargatesInRange()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // our scan won't work unless we know about our own designs
            playerIntel.Discover(player1, game.Designs[0]);

            // add a 250 ly stargate
            var planet1 = game.Planets[0];
            planet1.Starbase.Design.Slots.Add(new ShipDesignSlot(Techs.Stargate100_250, 1, 1));

            // add a stargate to detect
            var planet2 = game.Planets[1];
            planet2.Starbase.Design.Slots.Add(new ShipDesignSlot(Techs.Stargate100_250, 1, 1));

            // move the planet in range
            planet2.Position = new Vector2(250, 0);

            game.UpdateInternalDictionaries();
            gameRunner.ComputeSpecs(recompute: true);

            // we should discover this fleet
            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);

            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // update our various "ByGuid" dicts
            player1.SetupMapObjectMappings();

            // verify we didn't pen scan this planet
            var player1Planet2 = player1.PlanetsByGuid[planet2.Guid];
            Assert.IsFalse(player1Planet2.HasStarbase);
            Assert.IsNull(player1Planet2.Hab);

            // make our race IT, so we discover starbases
            player1.Race.PRT = PRT.IT;
            gameRunner.ComputeSpecs();

            game.UpdateInternalDictionaries();
            gameRunner.ComputeSpecs();

            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            Assert.IsTrue(player1Planet2.HasStarbase);
            Assert.AreEqual(new Hab(50, 50, 50), player1Planet2.Hab);
            Assert.Greater(player1Planet2.Population, 0);
            Assert.AreEqual(player2.Num, player1Planet2.PlayerNum);
        }

        [Test]
        public void TestDetectAllPackets()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            game.MineralPackets.Add(new MineralPacket()
            {
                PlayerNum = player2.Num,
                Position = new Vector2(1000, 1000),
                Cargo = new Cargo(1, 2, 3),
                WarpFactor = 10,
                Heading = new Vector2(1, 0),
                Target = game.Planets[0]
            });

            var scanStep = new PlayerScanStep(gameRunner.GameProvider, rulesProvider, playerIntel, playerTechService, fleetSpecService);
            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // should have nothing
            Assert.IsEmpty(player1.MineralPackets);

            player1.Race.PRT = PRT.PP;
            gameRunner.ComputeSpecs();

            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // should now know there is a packet coming towards us
            Assert.IsNotEmpty(player1.ForeignMineralPackets);
            Assert.AreEqual(new Vector2(1000, 1000), player1.ForeignMineralPackets[0].Position);
            Assert.AreEqual(new Vector2(1, 0), player1.ForeignMineralPackets[0].Heading);
            Assert.AreEqual(new Cargo(1, 2, 3), player1.ForeignMineralPackets[0].Cargo);

        }
    }

}