using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// TurnGenerationStep to transfer fleets from one player to another
    /// </summary>
    public class FleetTransferStep : AbstractFleetWaypointProcessStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetTransferStep));

        private readonly FleetSpecService fleetSpecService;

        public FleetTransferStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetSpecService fleetSpecService)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetTransferStep, WaypointTask.TransferFleet)
        {
            this.fleetSpecService = fleetSpecService;
        }

        /// <summary>
        /// Merge this fleet with another fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="wp"></param>
        /// <param name="player"></param>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            var targetPlayerNum = wp.TransferToPlayer;
            if (targetPlayerNum >= 0 && targetPlayerNum < Game.Players.Count)
            {
                var targetPlayer = Game.Players[targetPlayerNum];
                // transfer to the player and recompute the spec
                fleet.PlayerNum = targetPlayerNum;
                fleet.RaceName = targetPlayer.Race.Name;
                fleet.RacePluralName = targetPlayer.Race.PluralName;
                fleetSpecService.ComputeFleetSpec(targetPlayer, fleet, recompute: true);

                return true;
            }
            else
            {
                log.Error($"{Game.Year}: {player} tried to transfer fleet {fleet.Name} to player {targetPlayerNum} who doesn't exist.");
            }

            return false;
        }

    }
}