
namespace CraigStars
{
    /// <summary>
    /// Age fleets by one year
    /// </summary>
    public class FleetAgeStep : TurnGenerationStep
    {
        public FleetAgeStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetAgeStep) { }

        public override void Process()
        {
            Game.Fleets.ForEach(fleet => fleet.Age++);
        }

    }
}