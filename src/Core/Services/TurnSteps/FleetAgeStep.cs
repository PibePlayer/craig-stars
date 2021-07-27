
namespace CraigStars
{
    /// <summary>
    /// Age fleets by one year
    /// </summary>
    public class FleetAgeStep : TurnGenerationStep
    {
        public FleetAgeStep(Game game) : base(game, TurnGenerationState.FleetAge) { }

        public override void Process()
        {
            Game.Fleets.ForEach(fleet => fleet.Age++);
        }

    }
}