using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetTest
    {
        Rules rules = new Rules(0);

        [Test]
        public void TestGetMaxMines()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            Assert.AreEqual(0, planet.MaxMines);

            var player = new Player();
            planet.Player = player;
            planet.Population = 10000;
            Assert.AreEqual(10, planet.MaxMines);
        }

        [Test]
        public void TestGetMaxFactories()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            Assert.AreEqual(0, planet.MaxFactories);

            var player = new Player();
            planet.Player = player;
            planet.Population = 10000;
            Assert.AreEqual(10, planet.MaxFactories);
        }

        [Test]
        public void TestGetMaxDefenses()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            Assert.AreEqual(0, planet.MaxDefenses);

            var player = new Player();
            planet.Player = player;
            planet.Population = 10000;
            Assert.AreEqual(100, planet.MaxDefenses);
        }

        [Test]
        public void TestGetMaxPopulation()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);

            var player = new Player();

            player.Race.PRT = PRT.IS;
            Assert.AreEqual(1000000, planet.GetMaxPopulation(player.Race, rules));

            player.Race.PRT = PRT.JoaT;
            Assert.AreEqual(1200000, planet.GetMaxPopulation(player.Race, rules));

            player.Race.PRT = PRT.HE;
            Assert.AreEqual(500000, planet.GetMaxPopulation(player.Race, rules));

        }

        [Test]
        public void TestGetPopulationDensity()
        {
            var player = new Player();
            player.Race.PRT = PRT.IS;

            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);
            planet.Player = player;
            planet.Population = 100000;

            Assert.AreEqual(.1f, planet.PopulationDensity);
        }

        [Test]
        public void TestGetDefenseCoverage()
        {
            var defense = new TechDefense() { DefenseCoverage = .99f };

            var planet = new Planet()
            {
                Player = new Player(),
                Defenses = 10,
                Population = 10000
            };

            // should be about 9.5%
            Assert.AreEqual(.095f, planet.GetDefenseCoverage(defense), .001);
        }

        [Test]
        public void TestResourcesPerYear()
        {
            var planet = new Planet()
            {
                Player = new Player(),
                Factories = 10,
                Population = 25000
            };

            Assert.AreEqual(35, planet.ResourcesPerYear);
        }


        [Test]
        public void TestResourcesPerYearAvailable()
        {
            var planet = new Planet()
            {
                Player = new Player(),
                Factories = 10,
                Population = 25000,
                ContributesOnlyLeftoverToResearch = false
            };
            planet.Player.ResearchAmount = 15;

            Assert.AreEqual(30, planet.ResourcesPerYearAvailable);
            Assert.AreEqual(5, planet.ResourcesPerYearResearch);

            planet.ContributesOnlyLeftoverToResearch = true;
            Assert.AreEqual(35, planet.ResourcesPerYearAvailable);
            Assert.AreEqual(0, planet.ResourcesPerYearResearch);
        }

        [Test]
        public void TestGetQuantityToBuild()
        {
            Player player = new Player()
            {
                // we have grav3 terraforming ability
                TechLevels = new TechLevel(propulsion: 1, biotechnology: 1),
            };

            Planet planet = new Planet()
            {
                Player = player,
                Population = 100000, // 100k people
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50), // perfect planet
                Mines = 50 // start with 100 mines
            };

            // planet supports 1200 mines, so we can build up to 1150 more
            Assert.AreEqual(1150, planet.GetQuantityToBuild(2000, QueueItemType.Mine));

            // current population only supports 100 mines, so auto mines should only build up to 100 (50 more)
            Assert.AreEqual(50, planet.GetQuantityToBuild(2000, QueueItemType.AutoMines));

            // test terraform limits
            // we need 2 grav to get us habitable, but we can do up to 3
            planet.BaseHab = new Hab(13, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(3, planet.GetQuantityToBuild(5, QueueItemType.TerraformEnvironment));
            Assert.AreEqual(2, planet.GetQuantityToBuild(5, QueueItemType.AutoMinTerraform));
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
                Player = player,
                Hab = new Hab(50, 50, 50),
                BaseHab = new Hab(50, 50, 50),
            };

            // can't terraform a perfect planet
            Assert.AreEqual(new Hab(), planet.GetTerraformAmount());

            // this is off by 1, we can terraform
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), planet.GetTerraformAmount());

            // test the other direction
            planet.BaseHab = new Hab(55, 50, 50);
            planet.Hab = new Hab(55, 50, 50);
            Assert.AreEqual(new Hab(grav: -3), planet.GetTerraformAmount());

            // this is off by 10, but we've already terraformed 3, so we can't terraform anymore
            planet.BaseHab = new Hab(37, 50, 50);
            planet.Hab = new Hab(40, 50, 50);
            Assert.AreEqual(new Hab(), planet.GetTerraformAmount());

            // this is off by 10, we've already terraformed 2, so we can do one more
            planet.BaseHab = new Hab(37, 50, 50);
            planet.Hab = new Hab(39, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), planet.GetTerraformAmount());

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
                Player = player,
                Hab = new Hab(50, 50, 50),
                BaseHab = new Hab(50, 50, 50),
            };

            // can't terraform a perfect planet
            Assert.AreEqual(new Hab(), planet.GetMinTerraformAmount());

            // this is off by 1, but in range, so return no terraform needed
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(new Hab(), planet.GetMinTerraformAmount());

            // test the other direction, still in range, no terraform needed
            planet.BaseHab = new Hab(55, 50, 50);
            planet.Hab = new Hab(55, 50, 50);
            Assert.AreEqual(new Hab(), planet.GetMinTerraformAmount());


            // this is 2 points out of range, return 2
            planet.BaseHab = new Hab(13, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(new Hab(grav: 2), planet.GetMinTerraformAmount());

            // this is out of range by 4, but we've already terraformed 2 so we can terraform one more (still a bad planet)
            planet.BaseHab = new Hab(11, 50, 50);
            planet.Hab = new Hab(13, 50, 50);
            Assert.AreEqual(new Hab(grav: 1), planet.GetMinTerraformAmount());

        }

        [Test]
        public void GetBestTerraform()
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
                Player = player
            };

            // can't terraform a perfect planet
            Assert.AreEqual(null, planet.GetBestTerraform());

            // this is off by 1, we can terraform
            planet.BaseHab = new Hab(49, 50, 50);
            planet.Hab = new Hab(49, 50, 50);
            Assert.AreEqual(HabType.Gravity, planet.GetBestTerraform());

            // test to make sure we pick temperature because it's farther away
            planet.BaseHab = new Hab(55, 60, 50);
            planet.Hab = new Hab(55, 60, 50);
            Assert.AreEqual(HabType.Temperature, planet.GetBestTerraform());

            // test to make sure we pick radiation because it's the only one we can still terraform
            planet.BaseHab = new Hab(60, 60, 60);
            planet.Hab = new Hab(55, 55, 56);
            Assert.AreEqual(HabType.Radiation, planet.GetBestTerraform());

            // return null if we can't terraform anymore
            planet.BaseHab = new Hab(60, 60, 50);
            planet.Hab = new Hab(55, 55, 50);
            Assert.AreEqual(null, planet.GetBestTerraform());

        }
    }

}