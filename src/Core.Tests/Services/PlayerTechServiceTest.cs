using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerTechServiceTest
    {
        Rules rules = new Rules(0);

        PlayerTechService playerTechService;

        [SetUp]
        public void SetUp()
        {
            playerTechService = new PlayerTechService(TestUtils.TestContainer.GetInstance<IProvider<ITechStore>>());
        }

        [Test]
        public void TestGetBestOrbitalConstructionHull()
        {
            var player = new Player();
            player.Race.PRT = PRT.AR;
            Assert.AreEqual(Techs.OrbitalFort, playerTechService.GetBestOrbitalConstructionHull(player));
        }

        [Test]
        public void TestGetBestMineRobot()
        {
            var player = new Player();

            // should get RoboMiniMiner
            player.TechLevels = new TechLevel(construction: 2, electronics: 1);
            Assert.AreEqual(Techs.RoboMiniMiner, playerTechService.GetBestMineRobot(player));

            player.Race.LRTs.Add(LRT.ARM);
            Assert.AreEqual(Techs.RoboMidgetMiner, playerTechService.GetBestMineRobot(player));
         
            // JoaT with extra techs start at 4...
            player.TechLevels = new TechLevel(construction: 4, electronics: 4);
            Assert.AreEqual(Techs.RoboMiner, playerTechService.GetBestMineRobot(player));
        }

        [Test]
        public void TestGetBestEngine()
        {
            var player = new Player();

            // should get the lowest of the low
            player.TechLevels = new TechLevel();
            Assert.AreEqual(Techs.QuickJump5, playerTechService.GetBestEngine(player));

            player.TechLevels = new TechLevel(propulsion: 3);
            Assert.AreEqual(Techs.LongHump6, playerTechService.GetBestEngine(player));

            player.TechLevels = new TechLevel(propulsion: 3);
            player.Race.LRTs.Add(LRT.IFE);
            Assert.AreEqual(Techs.FuelMizer, playerTechService.GetBestEngine(player));

            player.TechLevels = new TechLevel(propulsion: 5);
            player.Race.LRTs.Add(LRT.IFE);
            Assert.AreEqual(Techs.DaddyLongLegs7, playerTechService.GetBestEngine(player));

            // JoaT with IFE and CE for prop 6
            player.TechLevels = new TechLevel(energy: 4, propulsion: 6);
            player.Race.LRTs.Add(LRT.IFE);
            player.Race.LRTs.Add(LRT.CE);
            Assert.AreEqual(Techs.RadiatingHydroRamScoop, playerTechService.GetBestEngine(player));
            // don't use the Radiating Hydro RamScoop for colonists
            Assert.AreEqual(Techs.DaddyLongLegs7, playerTechService.GetBestEngine(player, colonistTransport: true));
         
            // actually, we are immune to radiation damage, GIMME THAT RAMSCOOP!
            player.Race.ImmuneRad = true;
            Assert.AreEqual(Techs.RadiatingHydroRamScoop, playerTechService.GetBestEngine(player));
        }
    }
}