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
    public class ProductionQueueEstimatorTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(ProductionQueueEstimatorTest));


        PlanetService planetService;
        RaceService raceService;

        ProductionQueueEstimator estimator;

        [SetUp]
        public void SetUp()
        {
            planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            raceService = TestUtils.TestContainer.GetInstance<RaceService>();

            estimator = new ProductionQueueEstimator(
              planetService,
              TestUtils.TestContainer.GetInstance<PlayerService>()
          );
        }

        [Test]
        public void TestCalculateCompletionEstimatesSingleItem()
        {
            // create a starter homeworld with 35 resources / year
            var player = new Player();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Planet planet = new Planet()
            {
                PlayerNum = player.Num,
                Hab = new Hab(50, 50, 50),
                Population = 25000,
                Mines = 10,
                Factories = 10
            };
            planet.Spec = planetService.ComputePlanetSpec(planet, player);

            var item1 = new ProductionQueueItem(QueueItemType.Mine, 1);
            List<ProductionQueueItem> items = new List<ProductionQueueItem>()
            {
                item1,
            };

            // single mine should complete in 1 turn
            estimator.CalculateCompletionEstimates(planet, player, items, true);
            Assert.AreEqual(1, item1.yearsToBuildAll);
            Assert.AreEqual(1, item1.yearsToBuildOne);

            // 14 mines 5 resources each are 1 turn for one mine, 2 turns for all
            item1.Quantity = 14;
            estimator.CalculateCompletionEstimates(planet, player, items, true);
            Assert.AreEqual(1, item1.yearsToBuildOne);
            Assert.AreEqual(2, item1.yearsToBuildAll);
        }

        [Test]
        public void TestCalculateCompletionEstimatesManyItems()
        {
            // create a world with 10 factory resources per year and 90 colonist resources
            // for 100 resourcs per year total
            var player = new Player();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Planet planet = new Planet()
            {
                PlayerNum = player.Num,
                Hab = new Hab(50, 50, 50),
                Population = 90000,
                Factories = 10,
            };

            // give the planet a bunch of germanium to build factories with
            planet.Cargo = new Cargo(germanium: 1000).WithColonists(planet.Cargo.Colonists);
            planet.Spec = planetService.ComputePlanetSpec(planet, player);

            // 40 mines cost 200 resources, so should take 2 turns
            var item1 = new ProductionQueueItem(QueueItemType.Mine, 40);

            // 30 factories cost 300 resources so should take 3 turns
            var item2 = new ProductionQueueItem(QueueItemType.Factory, 30);
            List<ProductionQueueItem> items = new List<ProductionQueueItem>()
            {
                item1,
                item2,
            };

            estimator.CalculateCompletionEstimates(planet, player, items, true);
            Assert.AreEqual(1, item1.yearsToBuildOne);
            Assert.AreEqual(2, item1.yearsToBuildAll);

            // factories won't finish until mines are done, so the first
            // one will finish on year 3, then all will finish in 6 years total (3 years later)
            Assert.AreEqual(3, item2.yearsToBuildOne);
            Assert.AreEqual(5, item2.yearsToBuildAll);
        }
    }
}