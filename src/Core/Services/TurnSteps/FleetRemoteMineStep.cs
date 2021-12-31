using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// AR races remote mine their own planets at an earlier phase.
    /// </summary>
    public class FleetRemoteMineARStep : FleetRemoteMineStep
    {
        public FleetRemoteMineARStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, PlanetDiscoverer planetDiscoverer)
            : base(gameProvider, rulesProvider, planetService, planetDiscoverer, TurnGenerationState.RemoteMineARStep)
        {
        }

        /// <summary>
        /// Override Process so we only process remote mining tasks on our own worlds
        /// </summary>
        public override void Process()
        {
            // Separate our waypoint tasks into groups
            foreach (var task in Game.Fleets
                .Where(fleet => fleet.Orbiting != null && fleet.Orbiting.OwnedBy(fleet.PlayerNum) && fleet.Spec.MiningRate > 0 && Game.Players[fleet.PlayerNum].Race.Spec.CanRemoteMineOwnPlanets)
                .Select(fleet => BuildWaypointTasks(fleet, task))
                .Where(t => t != null)
            )
            {
                if (ProcessTask(task))
                {
                    CompleteWaypointForTurn(task.Waypoint);
                }
            }
        }
    }

    /// <summary>
    /// Regular remote mining happens only for wp0 remote mining tasks
    /// </summary>
    public class FleetRemoteMine0Step : FleetRemoteMineStep
    {
        public FleetRemoteMine0Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, PlanetDiscoverer planetDiscoverer)
            : base(gameProvider, rulesProvider, planetService, planetDiscoverer, TurnGenerationState.RemoteMine0Step)
        {
        }
    }

    /// <summary>
    /// Remote mine this planet
    /// </summary>
    public abstract class FleetRemoteMineStep : AbstractFleetWaypointProcessStep
    {
        private readonly PlanetService planetService;
        private readonly PlanetDiscoverer planetDiscoverer;

        public FleetRemoteMineStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, PlanetDiscoverer planetDiscoverer, TurnGenerationState state) : base(gameProvider, rulesProvider, state, WaypointTask.RemoteMining)
        {
            this.planetService = planetService;
            this.planetDiscoverer = planetDiscoverer;
        }

        /// <summary>
        /// Remoting mining tasks happen at the beginning of WP1
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;
            var planet = fleet.Orbiting;

            if (ValidateRemoteMining(fleet, wp, planet))
            {
                // no remote mining if we moved here this round
                if (!fleet.PreviousPosition.HasValue || fleet.PreviousPosition.Value == fleet.Position)
                {
                    // remote mine!
                    int numMines = fleet.Spec.MiningRate;
                    var mineralOutput = planetService.GetMineralOutput(planet, numMines);
                    planet.Cargo += mineralOutput;
                    planet.MineYears += numMines;
                    planetService.ReduceMineralConcentration(planet);
                    planetDiscoverer.DiscoverRemoteMined(player, planet);
                    Message.RemoteMined(player, fleet, planet, mineralOutput);
                }
            }
            else
            {
                // cancel this task
                fleet.Waypoints[0].Task = WaypointTask.None;
            }

            return true;
        }


        /// <summary>
        /// Validate remote mining orders
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="waypoint"></param>
        /// <param name="planet"></param>
        /// <returns></returns>
        internal bool ValidateRemoteMining(Fleet fleet, Waypoint waypoint, Planet planet)
        {
            if (planet == null)
            {
                Message.RemoteMineDeepSpace(Game.Players[fleet.PlayerNum], fleet);
                return false;
            }

            if (planet.Owned)
            {
                // AR races can remote mine their own planets
                if (!Game.Players[fleet.PlayerNum].Race.Spec.CanRemoteMineOwnPlanets && planet.PlayerNum == fleet.PlayerNum)
                {
                    Message.RemoteMineInhabited(Game.Players[fleet.PlayerNum], fleet, planet);
                    return false;
                }
            }

            if (fleet.Spec.MiningRate == 0)
            {
                Message.RemoteMineNoMiners(Game.Players[fleet.PlayerNum], fleet, planet);
                return false;
            }

            return true;
        }
    }
}