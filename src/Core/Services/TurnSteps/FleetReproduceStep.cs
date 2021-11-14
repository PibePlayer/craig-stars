
using System;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class FleetReproduceStep : TurnGenerationStep
    {
        private readonly PlayerService playerService;

        public FleetReproduceStep(IProvider<Game> gameProvider, PlayerService playerService) : base(gameProvider, TurnGenerationState.Grow)
        {
            this.playerService = playerService;
        }

        public override void Process()
        {
            // for any IS fleets that have colonists and cargo space, grow colonists
            foreach (Fleet fleet in Game.Fleets.Where(
                fleet => fleet.Cargo.Colonists > 0 &&
                playerService.FleetsReproduce(Game.Players[fleet.PlayerNum].Race)))
            {
                Reproduce(fleet, Game.Players[fleet.PlayerNum]);
            }
        }

        /// <summary>
        /// Reproducce colonists on this fleet
        /// </summary>
        /// <param name="fleet"></param>
        internal void Reproduce(Fleet fleet, Player player)
        {
            var growthFactor = playerService.GetFreighterGrowthFactor(player.Race);
            var growth = (int)(growthFactor * player.Race.GrowthRate / 100f * fleet.Cargo.Colonists);
            fleet.Cargo = fleet.Cargo.WithColonists(fleet.Cargo.Colonists + growth);
            var over = fleet.Cargo.Total - fleet.Aggregate.CargoCapacity;
            if (over > 0)
            {
                // remove excess colonists
                fleet.Cargo = fleet.Cargo.WithColonists(fleet.Cargo.Colonists - over);
                if (fleet.Orbiting != null && fleet.Orbiting.OwnedBy(fleet.PlayerNum))
                {
                    // add colonists to the planet this fleet is orbiting
                    fleet.Orbiting.Population += over * 100;
                }
            }

            // Message the player
            Message.FleetReproduce(player, fleet, growth, fleet.Orbiting, over);
        }
    }
}