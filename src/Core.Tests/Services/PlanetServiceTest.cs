using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetServiceTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetServiceTest));

        Rules rules = new Rules(0);
        PlanetService service = new PlanetService(
            TestUtils.TestContainer.GetInstance<PlayerService>(),
            TestUtils.TestContainer.GetInstance<PlayerTechService>(),
            TestUtils.TestContainer.GetInstance<IRulesProvider>()
        );

        RaceService raceService = new RaceService(new TestRulesProvider());

        [Test]
        public void TestGetMaxMines()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            var player = new Player();
            planet.Population = 10000;
            Assert.AreEqual(10, service.GetMaxMines(planet, player));
        }

        [Test]
        public void TestGetMaxFactories()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            var player = new Player();
            planet.Population = 10000;
            Assert.AreEqual(10, service.GetMaxFactories(planet, player));
        }

        [Test]
        public void TestGetMaxDefenses()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            var player = new Player();
            planet.PlayerNum = player.Num;
            planet.Population = 10000;
            Assert.AreEqual(100, service.GetMaxDefenses(planet, player));
        }

        [Test]
        public void TestGetMaxPopulation()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);

            var player = new Player();

            player.Race.PRT = PRT.IS;
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(1000000, service.GetMaxPopulation(planet, player));

            player.Race.PRT = PRT.JoaT;
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(1200000, service.GetMaxPopulation(planet, player));

            player.Race.PRT = PRT.HE;
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(500000, service.GetMaxPopulation(planet, player));

        }

        [Test]
        public void TestGrowthAmount()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);
            var player = new Player();
            player.Race.GrowthRate = 10;
            planet.PlayerNum = player.Num;
            planet.Population = 100_000;

            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);

            // less than 25% cap, grows at full 10% growth rate
            Assert.AreEqual(10_000, service.GetGrowthAmount(planet, player));

            // at 50% cap, it slows down in growth
            planet.Population = 600_000;
            Assert.AreEqual(26700, service.GetGrowthAmount(planet, player));

            // we are basicallly at capacity, we only grow a tiny amount
            planet.Population = 1_180_000;
            Assert.AreEqual(100, service.GetGrowthAmount(planet, player));

            // no more growth past a certain capacity
            planet.Population = 1_190_000;
            Assert.AreEqual(0, service.GetGrowthAmount(planet, player));

            // hostile planets kill off colonists
            planet.Hab = new Hab(10, 15, 15);
            planet.Population = 2_500;
            Assert.AreEqual(-100, service.GetGrowthAmount(planet, player));

            // super hostile planet with 100k people
            // should be -45% habitable, so should kill off -4.5% of the pop
            planet.Hab = new Hab(0, 0, 0);
            planet.Population = 100_000;
            Assert.AreEqual(-4500, service.GetGrowthAmount(planet, player));

            // TODO: terraforming planets don't die off or grow if planet is negative

        }

        [Test]
        public void TestGetPopulationDensity()
        {
            var player = new Player();
            player.Race.PRT = PRT.IS;

            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);
            planet.PlayerNum = player.Num;
            planet.Population = 100000;

            Assert.AreEqual(.1f, service.GetPopulationDensity(planet, player, rules));
        }

        [Test]
        public void TestGetDefenseCoverage()
        {
            var defense = new TechDefense() { DefenseCoverage = .99f };
            var player = new Player();

            var planet = new Planet()
            {
                Defenses = 10,
                Population = 10000
            };

            // should be about 9.5%
            Assert.AreEqual(.095f, service.GetDefenseCoverage(planet, player, defense), .001);
        }

        [Test]
        public void TestGetQuantityToBuild()
        {
            Player player = new Player()
            {
                // we have grav3 terraforming ability
                TechLevels = new TechLevel(propulsion: 1, biotechnology: 1),
            };
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);

            Planet planet = new Planet()
            {
                PlayerNum = player.Num,
                Population = 100000, // 100k people
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50), // perfect planet
                Mines = 50 // start with 100 mines
            };

            // planet supports 1200 mines, so we can build up to 1150 more
            Assert.AreEqual(1150, service.GetQuantityToBuild(planet, player, 2000, QueueItemType.Mine));

            // current population only supports 100 mines, so auto mines should only build up to 100 (50 more)
            Assert.AreEqual(50, service.GetQuantityToBuild(planet, player, 2000, QueueItemType.AutoMines));

            // test terraform limits
            // we need 2 grav to get us habitable, but we can do up to 3
            planet.BaseHab = new Hab(13, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(3, service.GetQuantityToBuild(planet, player, 5, QueueItemType.TerraformEnvironment));
            Assert.AreEqual(2, service.GetQuantityToBuild(planet, player, 5, QueueItemType.AutoMinTerraform));
        }

        [Test]
        public void TestGetTerraformAmount()
        {
            // Create a basic player with tech levels for Gravity3 terraform
            var player = new Player()
            {
                Race = new Race()
                {
                    HabLow = new Hab(15, 15, 15),
                    HabHigh = new Hab(85, 85, 85),
                },
                TechLevels = new TechLevel(propulsion: 1, biotechnology: 1),
            };

            // create a perfect planet
            var planet = new Planet()
            {
                PlayerNum = player.Num,
                Hab = new Hab(50, 50, 50),
                BaseHab = new Hab(50, 50, 50),
            };

            // can't terraform a perfect planet
            Assert.AreEqual(new Hab(), service.GetTerraformAmount(planet, player));

            // this is off by 1, we can terraform
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), service.GetTerraformAmount(planet, player));

            // test the other direction
            planet.BaseHab = new Hab(55, 50, 50);
            planet.Hab = new Hab(55, 50, 50);
            Assert.AreEqual(new Hab(grav: -3), service.GetTerraformAmount(planet, player));

            // this is off by 10, but we've already terraformed 3, so we can't terraform anymore
            planet.BaseHab = new Hab(37, 50, 50);
            planet.Hab = new Hab(40, 50, 50);
            Assert.AreEqual(new Hab(), service.GetTerraformAmount(planet, player));

            // this is off by 10, we've already terraformed 2, so we can do one more
            planet.BaseHab = new Hab(37, 50, 50);
            planet.Hab = new Hab(39, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), service.GetTerraformAmount(planet, player));

        }

        [Test]
        public void TestGetMinTerraformAmount()
        {
            // Create a basic player with tech levels for Gravity3 terraform
            var player = new Player()
            {
                Race = new Race()
                {
                    HabLow = new Hab(15, 15, 15),
                    HabHigh = new Hab(85, 85, 85),
                },
                TechLevels = new TechLevel(propulsion: 1, biotechnology: 1),
            };

            // create a perfect planet
            var planet = new Planet()
            {
                PlayerNum = player.Num,
                Hab = new Hab(50, 50, 50),
                BaseHab = new Hab(50, 50, 50),
            };

            // can't terraform a perfect planet
            Assert.AreEqual(new Hab(), service.GetMinTerraformAmount(planet, player));

            // this is off by 1, but in range, so return no terraform needed
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(new Hab(), service.GetMinTerraformAmount(planet, player));

            // test the other direction, still in range, no terraform needed
            planet.BaseHab = new Hab(55, 50, 50);
            planet.Hab = new Hab(55, 50, 50);
            Assert.AreEqual(new Hab(), service.GetMinTerraformAmount(planet, player));


            // this is 2 points out of range, return 2
            planet.BaseHab = new Hab(13, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(new Hab(grav: 2), service.GetMinTerraformAmount(planet, player));

            // this is out of range by 4, but we've already terraformed 2 so we can terraform one more (still a bad planet)
            planet.BaseHab = new Hab(11, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), service.GetMinTerraformAmount(planet, player));

        }

        [Test]
        public void TestGetBestTerraform()
        {
            // Create a basic player with tech levels for Gravity3 terraform
            var player = new Player()
            {
                Race = new Race()
                {
                    HabLow = new Hab(15, 15, 15),
                    HabHigh = new Hab(85, 85, 85),
                    LRTs = new HashSet<LRT>() { LRT.TT }
                },
                TechLevels = new TechLevel(biotechnology: 3),
            };

            // create a perfect planet
            var planet = new Planet()
            {
                Hab = new Hab(50, 50, 50),
                BaseHab = new Hab(50, 50, 50),
                PlayerNum = player.Num
            };

            // can't terraform a perfect planet
            Assert.AreEqual(null, service.GetBestTerraform(planet, player));

            // this is off by 1, we can terraform
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(HabType.Gravity, service.GetBestTerraform(planet, player));

            // test to make sure we pick temperature because it's farther away
            planet.BaseHab = new Hab(55, 60, 50);
            planet.Hab = new Hab(55, 60, 50);
            Assert.AreEqual(HabType.Temperature, service.GetBestTerraform(planet, player));

            // test to make sure we pick radiation because it's the only one we can still terraform
            planet.BaseHab = new Hab(60, 60, 60);
            planet.Hab = new Hab(55, 55, 56);
            Assert.AreEqual(HabType.Radiation, service.GetBestTerraform(planet, player));

            // return null if we can't terraform anymore
            planet.BaseHab = new Hab(60, 60, 50);
            planet.Hab = new Hab(55, 55, 50);
            Assert.AreEqual(null, service.GetBestTerraform(planet, player));

        }

        [Test]
        public void TestResourcesPerYear()
        {
            var player = new Player();
            var planet = new Planet()
            {
                Factories = 10,
                Population = 25000
            };

            Assert.AreEqual(35, service.GetResourcesPerYear(planet, player));
        }


        [Test]
        public void TestResourcesPerYearAvailable()
        {
            var player = new Player();
            var planet = new Planet()
            {
                Factories = 10,
                Population = 25000,
                ContributesOnlyLeftoverToResearch = false
            };
            player.ResearchAmount = 15;

            Assert.AreEqual(30, service.GetResourcesPerYearAvailable(planet, player));
            Assert.AreEqual(5, service.GetResourcesPerYearResearch(planet, player));

            planet.ContributesOnlyLeftoverToResearch = true;
            Assert.AreEqual(35, service.GetResourcesPerYearAvailable(planet, player));
            Assert.AreEqual(0, service.GetResourcesPerYearResearch(planet, player));
        }

        [Test]
        public void TestApplyProductionPlan()
        {
            var player = new Player()
            {
                ProductionPlans = new List<ProductionPlan>() {
                    new() {
                        Name = "Default",
                        Items = new() {
                            new ProductionQueueItem(QueueItemType.AutoFactories, 10),
                            new ProductionQueueItem(QueueItemType.AutoMines, 5)
                        }
                    }
                }
            };
            var planet = new Planet()
            {
                ProductionQueue = new()
            };

            // add a couple items
            service.ApplyProductionPlan(planet.ProductionQueue.Items, player, player.ProductionPlans[0]);
            Assert.AreEqual(2, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoFactories, 10), planet.ProductionQueue.Items[0]);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoMines, 5), planet.ProductionQueue.Items[1]);

            // make sure existing auto items are removed
            planet.ProductionQueue.Items = new()
            {
                new ProductionQueueItem(QueueItemType.AutoDefenses, 1)
            };

            service.ApplyProductionPlan(planet.ProductionQueue.Items, player, player.ProductionPlans[0]);
            Assert.AreEqual(2, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoFactories, 10), planet.ProductionQueue.Items[0]);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoMines, 5), planet.ProductionQueue.Items[1]);

            // make sure existing manul items items are not removed
            planet.ProductionQueue.Items = new()
            {
                // this should be gone
                new ProductionQueueItem(QueueItemType.AutoDefenses, 1),
                // this should remain
                new ProductionQueueItem(QueueItemType.Mine, 1)
            };

            service.ApplyProductionPlan(planet.ProductionQueue.Items, player, player.ProductionPlans[0]);
            Assert.AreEqual(3, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.Mine, 1), planet.ProductionQueue.Items[0]);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoFactories, 10), planet.ProductionQueue.Items[1]);
            Assert.AreEqual(new ProductionQueueItem(QueueItemType.AutoMines, 5), planet.ProductionQueue.Items[2]);

        }
    }
}
