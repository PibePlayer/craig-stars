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
    }
}