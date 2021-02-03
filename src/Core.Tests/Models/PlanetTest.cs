using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetTest
    {
        Rules rules = new Rules();

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
            Assert.AreEqual(10, planet.MaxDefenses);
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

            Assert.AreEqual(.1f, planet.GetPopulationDensity(rules));
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
    }

}