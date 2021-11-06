
namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class PlanetGrowStep : TurnGenerationStep
    {
        PlanetService planetService = new();
        public PlanetGrowStep(Game game) : base(game, TurnGenerationState.Grow) { }

        public override void Process()
        {
            OwnedPlanets.ForEach(p => p.Population += planetService.GetGrowthAmount(p, Game.Players[p.PlayerNum], Game.Rules));
        }

    }
}