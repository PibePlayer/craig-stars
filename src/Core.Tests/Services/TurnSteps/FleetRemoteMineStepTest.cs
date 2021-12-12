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
    public class FleetRemoteMineStepTest
    {

        IRulesProvider rulesProvider;
        PlanetService planetService;
        PlanetDiscoverer planetDiscover;

        [SetUp]
        public void SetUp()
        {
            rulesProvider = new TestRulesProvider();
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            planetDiscover = TestUtils.TestContainer.GetInstance<PlanetDiscoverer>();
        }

        [Test]
        public void TestRemoteMine()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            var planet = new Planet()
            {
                Name = "Planet To Remote Mine",
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
                Cargo = new Cargo(),
            };

            game.Planets.Add(planet);

            // build a remote mining fleet
            var miner = TestUtils.CreateDesign(game, player, ShipDesigns.CottonPicker);
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(miner, 1)
                },
            };
            game.Fleets.Add(fleet);

            planet.OrbitingFleets.Add(fleet);
            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, task: WaypointTask.RemoteMining));

            gameRunner.ComputeSpecs(recompute: true);

            var step = new FleetRemoteMine0Step(gameRunner.GameProvider, rulesProvider, planetService, planetDiscover);
            step.Process();

            // we have 8 miners, so we extract 8 minerals per turn
            Assert.AreEqual(new Cargo(8, 8, 8), planet.Cargo);

            // for a 100 conc planet, the mineral decay starts at 1_500_000 / 100 / 100 = 150 minerals mined.
            // we mine 8 a turn, so run it that many times
            for (int i = 0; i < 150 / 8 + 1; i++)
            {
                step = new FleetRemoteMine0Step(gameRunner.GameProvider, rulesProvider, planetService, planetDiscover);
                step.Process();
            }
            Assert.AreEqual(new Mineral(99, 99, 99), planet.MineralConcentration);
        }

        [Test]
        public void TestRemoteMineAR()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var planet = game.Planets[0];
            player.Race.PRT = PRT.AR;
            
            // build a remote mining fleet
            var miner = TestUtils.CreateDesign(game, player, ShipDesigns.CottonPicker);
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(miner, 1)
                },
            };
            game.Fleets.Add(fleet);

            planet.OrbitingFleets.Add(fleet);
            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, task: WaypointTask.RemoteMining));

            planet.MineralConcentration = new Mineral(100, 100, 100);

            gameRunner.ComputeSpecs(recompute: true);

            var context = new TurnGenerationContext();
            var step = new FleetRemoteMineARStep(gameRunner.GameProvider, rulesProvider, planetService, planetDiscover);
            step.Execute(context, game.OwnedPlanets.ToList());

            // we have 8 miners, so we extract 8 minerals per turn
            Assert.AreEqual(new Mineral(8, 8, 8), planet.Cargo.ToMineral());

            // make sure we don't mine twice
            var regularRemoteMineStep = new FleetRemoteMine0Step(gameRunner.GameProvider, rulesProvider, planetService, planetDiscover);
            regularRemoteMineStep.Execute(context, game.OwnedPlanets.ToList());

            // should be the same as before
            Assert.AreEqual(new Mineral(8, 8, 8), planet.Cargo.ToMineral());
        }
    }
}