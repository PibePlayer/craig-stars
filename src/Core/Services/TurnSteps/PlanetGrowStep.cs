
namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class PlanetGrowStep : TurnGenerationStep
    {
        private readonly PlanetService planetService;

        public PlanetGrowStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.Grow)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            OwnedPlanets.ForEach(p => p.Population += planetService.GetGrowthAmount(p, Game.Players[p.PlayerNum]));
        }

    }
}