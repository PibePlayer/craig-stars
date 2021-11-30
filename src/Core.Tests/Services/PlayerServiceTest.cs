using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerServiceTest
    {
        Rules rules = new Rules(0);

        PlayerService playerService;
        RaceService raceService = new RaceService(new TestRulesProvider());

        [SetUp]
        public void SetUp()
        {
            playerService = new PlayerService(new TestRulesProvider());
        }

        [Test]
        public void TestGetCost()
        {
            // a 100 ironium, 1 energy tech cost 100 at 1 energy level
            var tech = new TechHullComponent()
            {
                Requirements = new TechRequirements(energy: 1),
                Cost = new Cost(100, 0, 0, 0)
            };

            var player = new Player()
            {
                TechLevels = new TechLevel()
                {
                    Energy = 1
                }
            };

            // should cost 100
            Assert.AreEqual(100, playerService.GetTechCost(player, tech).Ironium);

            // only the lowest level over matters
            player.TechLevels.Biotechnology = 10;
            Assert.AreEqual(100, playerService.GetTechCost(player, tech).Ironium);

            // increasing past our minimum by 1 level reduces costs to 96% or original
            player.TechLevels.Energy = 2;
            Assert.AreEqual(96, playerService.GetTechCost(player, tech).Ironium);

        }

        [Test]
        public void TestGetTechCostStarterScout()
        {
            // a scout's base cost is 4, 2, 4, 9
            var tech = new TechHullComponent()
            {
                Requirements = new TechRequirements(),
                Cost = new Cost(4, 2, 4, 9)
            };

            var player = new Player()
            {
                TechLevels = new TechLevel()
                {
                    Energy = 16,
                    Weapons = 16,
                    Propulsion = 16,
                    Construction = 15,
                    Electronics = 15,
                    Biotechnology = 15,
                }
            };

            // should cost 2, 1, 2, 4 at these tech levels
            Assert.AreEqual(new Cost(2, 1, 2, 4), playerService.GetTechCost(player, tech));
        }


        [Test]
        public void TestGetTechCostBET()
        {
            // a 100 ironium, 1 energy tech cost 100 at 1 energy level
            var tech = new TechHullComponent()
            {
                Requirements = new TechRequirements(energy: 1),
                Cost = new Cost(100, 0, 0, 0)
            };

            var player = new Player()
            {
                TechLevels = new TechLevel()
                {
                    Energy = 1
                }
            };

            // bleeding edge tech costs double at lowest level
            player.Race.LRTs.Add(LRT.BET);
            player.TechLevels.Energy = 1;
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);

            Assert.AreEqual(200, playerService.GetTechCost(player, tech).Ironium);

            // bleeding edge tech costs 5% less per level
            player.TechLevels.Energy = 2;
            Assert.AreEqual(95, playerService.GetTechCost(player, tech).Ironium);
        }

        [Test]
        public void TestGetTechCostWM()
        {
            // a 100 ironium, 1 energy tech cost 100 at 1 energy level
            var tech = new TechHullComponent()
            {
                Category = TechCategory.BeamWeapon,
                Cost = new Cost(ironium: 100)
            };

            var player = new Player();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(100, playerService.GetTechCost(player, tech).Ironium);

            // Warmongers get cheaper weapons
            player.Race.PRT = PRT.WM;
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(75, playerService.GetTechCost(player, tech).Ironium);
        }

        [Test]
        public void TestGetTechCostCE()
        {
            // a 100 ironium, 1 energy tech cost 100 at 1 energy level
            var tech = new TechHullComponent()
            {
                Category = TechCategory.Engine,
                Cost = new Cost(ironium: 100)
            };

            var player = new Player();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(100, playerService.GetTechCost(player, tech).Ironium);

            // Warmongers get cheaper weapons
            player.Race.LRTs.Add(LRT.CE);
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            Assert.AreEqual(50, playerService.GetTechCost(player, tech).Ironium);
        }
    }

}