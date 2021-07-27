
namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class PlanetGrowStep : TurnGenerationStep
    {
        public PlanetGrowStep(Game game) : base(game, TurnGenerationState.Grow) { }

        public override void Process()
        {
            OwnedPlanets.ForEach(p => p.Population += p.GrowthAmount);
        }

    }
}