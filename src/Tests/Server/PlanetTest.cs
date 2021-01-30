using CraigStars.Singletons;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetTest
    {
        UniverseSettings settings = new UniverseSettings();
        [Test]
        public void TestGetMaxMines()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();

            Assert.AreEqual(0, planet.MaxMines);

            var player = new Player();
            planet.Player = player;
            planet.ProductionQueue = new ProductionQueue();
            planet.Population = 10000;
            Assert.AreEqual(10, planet.MaxMines);
        }

        [Test]
        public void TestGetMaxPopulation()
        {
            var planet = new Planet();
            planet.InitEmptyPlanet();
            planet.Hab = new Hab(50, 50, 50);

            var player = new Player();

            player.Race.PRT = PRT.IS;
            Assert.AreEqual(1000000, planet.GetMaxPopulation(player.Race, settings));

            player.Race.PRT = PRT.JoaT;
            Assert.AreEqual(1200000, planet.GetMaxPopulation(player.Race, settings));

            player.Race.PRT = PRT.HE;
            Assert.AreEqual(500000, planet.GetMaxPopulation(player.Race, settings));

        }
    }

}