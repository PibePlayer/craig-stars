using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetScrapperServiceTest
    {
        FleetScrapperService service;
        RaceService raceService;

        Game game;
        GameRunner gameRunner;
        Player player;
        Fleet fleet;
        Planet planet;

        [SetUp]
        public void SetUp()
        {
            service = new FleetScrapperService(new TestRulesProvider());
            raceService = TestUtils.TestContainer.GetInstance<RaceService>();

            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            player = game.Players[0];
            fleet = game.Fleets[0];
            planet = game.Planets[0];
        }

        [Test]
        public void TestScrapFleet()
        {
            Mineral mineralCost = fleet.Spec.Cost;

            service.ScrapFleet(player, fleet, planet);

            Assert.AreEqual(mineralCost * (1 / 3f), planet.Cargo.ToMineral());
            Assert.AreEqual(0, planet.BonusResources);

            gameRunner.OnPurgeDeletedMapObjects();
            Assert.AreEqual(0, game.Fleets.Count);
        }

        [Test]
        public void TestScrapFleetURStarbase()
        {
            player.Race.LRTs.Add(LRT.UR);
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            gameRunner.ComputeSpecs();

            Mineral mineralCost = fleet.Spec.Cost;
            int resourceCost = fleet.Spec.Cost.Resources;
            service.ScrapFleet(player, fleet, planet);

            Assert.AreEqual(mineralCost * (.9f), planet.Cargo.ToMineral());
            Assert.AreEqual(10, planet.BonusResources);
        }

        [Test]
        public void TestScrapFleetURNoStarbase()
        {
            player.Race.LRTs.Add(LRT.UR);
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            planet.Starbase = null;
            gameRunner.ComputeSpecs();

            Mineral mineralCost = fleet.Spec.Cost;
            int resourceCost = fleet.Spec.Cost.Resources;
            service.ScrapFleet(player, fleet, planet);

            Assert.AreEqual(mineralCost * (.45f), planet.Cargo.ToMineral());
            Assert.AreEqual(6, planet.BonusResources);
        }

        [Test]
        public void TestScrapFleetURDeepSpace()
        {
            game.MoveMapObject(fleet, fleet.Position, new Vector2(100, 100));
            gameRunner.ComputeSpecs();

            Mineral mineralCost = fleet.Spec.Cost;

            Salvage salvage = null;
            Action<MapObject> onSalvageCreated = (MapObject mo) =>
            {
                salvage = mo as Salvage;
            };

            EventManager.MapObjectCreatedEvent += onSalvageCreated;
            try
            {
                service.ScrapFleet(player, fleet, null);
            }
            finally
            {
                EventManager.MapObjectCreatedEvent -= onSalvageCreated;
            }

            // should create salvage with 1/3 minerals and give the player some resources
            Assert.NotNull(salvage);
            Assert.AreEqual(mineralCost * (1f / 3f), salvage.Cargo.ToMineral());
        }
    }
}
