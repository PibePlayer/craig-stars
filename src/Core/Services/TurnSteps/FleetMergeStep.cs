using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetMerge0Step : FleetMergeStep
    {
        public FleetMerge0Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetService fleetService)
            : base(gameProvider, rulesProvider, fleetService, TurnGenerationState.FleetMerge0Step)
        {
        }
    }

    public class FleetMerge1Step : FleetMergeStep
    {
        public FleetMerge1Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetService fleetService)
            : base(gameProvider, rulesProvider, fleetService, TurnGenerationState.FleetMerge1Step)
        {
        }
    }

    /// <summary>
    /// TurnGenerationStep to transfer fleets from one player to another
    /// </summary>
    public abstract class FleetMergeStep : AbstractFleetWaypointProcessStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetMergeStep));

        private readonly FleetService fleetService;

        public FleetMergeStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetService fleetService, TurnGenerationState state)
            : base(gameProvider, rulesProvider, state, WaypointTask.MergeWithFleet)
        {
            this.fleetService = fleetService;
        }

        /// <summary>
        /// Merge this fleet with another fleet
        /// </summary>
        /// <param name="task"></param>
        /// <returns>true (we message the player if the merge fails, but the waypoint is considered processed)</returns>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            var target = wp.Target as Fleet;
            if (target == null)
            {
                Message.FleetInvalidMergeNotFleet(player, fleet);
            }
            else if (target is Fleet targetFleet)
            {
                if (!targetFleet.OwnedBy(player))
                {
                    Message.FleetInvalidMergeNotOwned(player, fleet);
                }
                else
                {
                    // we're good, merge!
                    fleetService.Merge(targetFleet, player, new List<Fleet>() { fleet });
                    Message.FleetMerged(player, fleet, targetFleet);
                    EventManager.PublishMapObjectDeletedEvent(fleet);

                }
            }

            return true;
        }

    }
}