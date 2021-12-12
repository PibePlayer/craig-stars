using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetPatrolStep : AbstractFleetWaypointProcessStep
    {
        public FleetPatrolStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetPatrolStep, WaypointTask.Patrol)
        {
        }

        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            // TODO: Fill in
            return true;
        }
    }
}