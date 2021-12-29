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
    public class FleetBattleStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetBattleStepTest));

        FleetSpecService fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        FleetService fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
        ShipDesignDiscoverer designDiscoverer = TestUtils.TestContainer.GetInstance<ShipDesignDiscoverer>();

        [Test]
        public void TestStarbaseDestroyed()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();

            FleetBattleStep step = new FleetBattleStep(
                new Provider<Game>(game),
                new BattleEngine(game, fleetService, designDiscoverer),
                fleetSpecService
            );

            var player1 = game.Players[0];
            var player2Planet = game.Planets[1];

            var stalwartDefender = TestUtils.CreateDesign(game, player1, ShipDesigns.StalwartDefender);
            var attacker = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(stalwartDefender, 100)
                },
                BattlePlan = player1.BattlePlans[0],
                Position = player2Planet.Position,
                Orbiting = player2Planet,
            };
            game.AddMapObject(attacker);

            gameRunner.ComputeSpecs();


            Starbase destroyedStarbase = null;
            Action<MapObject> starbaseDestroyed = (mo) =>
            {
                destroyedStarbase ??= mo as Starbase;
            };
            EventManager.MapObjectDeletedEvent += starbaseDestroyed;
            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());
            EventManager.MapObjectDeletedEvent -= starbaseDestroyed;

            Assert.Greater(player2Planet.Population, 0);
            Assert.AreEqual(player2Planet.Starbase, destroyedStarbase);
        }

        [Test]
        public void TestARStarbaseDestroyed()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();

            FleetBattleStep step = new FleetBattleStep(
                new Provider<Game>(game),
                new BattleEngine(game, fleetService, designDiscoverer),
                fleetSpecService
            );

            var player1 = game.Players[0];
            var player2Planet = game.Planets[1];

            var stalwartDefender = TestUtils.CreateDesign(game, player1, ShipDesigns.StalwartDefender);
            var attacker = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(stalwartDefender, 100)
                },
                BattlePlan = player1.BattlePlans[0],
                Position = player2Planet.Position,
                Orbiting = player2Planet,
            };
            game.AddMapObject(attacker);

            // make player2 AR
            var player2 = game.Players[1];
            player2.Race.PRT = PRT.AR;

            gameRunner.ComputeSpecs();

            // run the battle
            Planet emptiedPlanet = null;
            Action<Planet> planetEmptied = (planet) =>
            {
                emptiedPlanet = planet;
            };

            EventManager.PlanetPopulationEmptiedEvent += planetEmptied;
            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());
            EventManager.PlanetPopulationEmptiedEvent -= planetEmptied;

            // should destroy the starbase and wipe the planet
            Assert.AreEqual(0, player2Planet.Population);
            Assert.AreEqual(emptiedPlanet, player2Planet);
        }
    }
}