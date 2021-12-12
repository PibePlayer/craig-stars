using Godot;

namespace CraigStars
{
    /// <summary>
    /// Mine planets for resources
    /// </summary>
    public class PlanetMineStep : TurnGenerationStep
    {
        private readonly PlanetService planetService;

        public PlanetMineStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.PlanetMineStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            OwnedPlanets.ForEach(p =>
            {
                p.Cargo += p.Spec.MineralOutput;
                p.MineYears += p.Mines;
                planetService.ReduceMineralConcentration(p);
            });
        }

    }
}