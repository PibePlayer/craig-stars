using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    /// <summary>
    /// This class holds tests to make sure all the PRT functionality is implemented
    /// </summary>
    [TestFixture]
    public class RaceTests
    {
        static CSLog log = LogProvider.GetLogger(typeof(RaceTests));

        Rules rules = new Rules(0);
        PlanetService planetService = new PlanetService(
            TestUtils.TestContainer.GetInstance<PlayerService>(),
            TestUtils.TestContainer.GetInstance<PlayerTechService>(),
            TestUtils.TestContainer.GetInstance<IRulesProvider>()
        );

        RaceService raceService = new RaceService(new TestRulesProvider());

        [Test]
        public void TestHE()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player = game.Players[0];
            player.Race.PRT = PRT.HE;
            player.Race.GrowthRate = 10;

            // update spec
            gameRunner.ComputeSpecs();

            // growth should be 2x
            var planet = game.Planets[0];
            planet.Population = 10000;
            planet.Spec = planetService.ComputePlanetSpec(planet, player);
            Assert.AreEqual(2000, planet.Spec.GrowthAmount);

            // max pop should be half
            Assert.AreEqual(500000, planet.Spec.MaxPopulation);
        }

        [Test]
        public void TestSS()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player = game.Players[0];
            player.Race.PRT = PRT.SS;
            gameRunner.ComputeSpecs(recompute: true);

            // 75% built in cloaking
            var fleet = game.Fleets[0];
            Assert.AreEqual(75, fleet.Spec.CloakPercent);

            var planet = game.Planets[0];
            planet.Starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(TestUtils.CreateDesign(game, player, ShipDesigns.Starbase.Clone(player)), 1)
                }
            };
            gameRunner.ComputeSpecs(recompute: true);
            Assert.AreEqual(75, planet.Starbase.Spec.CloakPercent);

            // travel through minefields at 1 warp better
            Assert.AreEqual(1, player.Race.Spec.MineFieldSafeWarpBonus);

            // stolen research tested in the PlayerResearchStepTest
        }
    }
}
