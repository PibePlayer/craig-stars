using NUnit.Framework;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetGrowStepTest
    {
        PlanetGrowStep step;
        Game game;
        GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new PlanetGrowStep(gameRunner.GameProvider, planetService);
        }

        [Test]
        public void TestGrow()
        {
            var planet = game.Planets[0];
            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // standard humanoid growth
            Assert.AreEqual(28_800, planet.Population);
            Assert.AreEqual(0, planet.Mines);
        }

        [Test]
        public void TestGrowAR()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            
            player.Race.PRT = PRT.AR;
            gameRunner.ComputeSpecs();

            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // standard humanoid growth
            Assert.AreEqual(28_800, planet.Population);
            Assert.AreEqual(16, planet.Mines);
        }

    }
}