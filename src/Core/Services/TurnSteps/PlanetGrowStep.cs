
namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class PlanetGrowStep : TurnGenerationStep
    {
        private readonly PlanetService planetService;

        public PlanetGrowStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.PlanetGrowStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            OwnedPlanets.ForEach(planet =>
            {
                planet.Population += planet.Spec.GrowthAmount;
                var player = Game.Players[planet.PlayerNum];
                if (player.Race.Spec.InnateMining) {
                    planet.Mines = planetService.GetInnateMines(planet, player);
                }
                planetService.ComputePlanetSpec(planet, Game.Players[planet.PlayerNum]);
            });
        }

    }
}