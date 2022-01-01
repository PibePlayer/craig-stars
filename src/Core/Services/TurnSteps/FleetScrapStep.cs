using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetScrapStep : AbstractFleetWaypointProcessStep
    {
        private readonly FleetScrapperService fleetScrapperService;
        public FleetScrapStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetScrapperService fleetScrapperService)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetScrapStep, WaypointTask.ScrapFleet)
        {
            this.fleetScrapperService = fleetScrapperService;
        }

        /// <summary>
        /// Execute scrap fleet waypoint tasks
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>    
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            fleetScrapperService.ScrapFleet(player, fleet, wp.Target as Planet);

            // this task is complete
            return true;
        }
    }
}