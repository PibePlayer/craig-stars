using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetRoute0Step : FleetRouteStep
    {
        public FleetRoute0Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetRoute0Step)
        {
        }
    }

    public class FleetRoute1Step : FleetRouteStep
    {
        public FleetRoute1Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetRoute1Step)
        {
        }
    }

    /// <summary>
    /// TurnGenerationStep to transfer fleets from one player to another
    /// </summary>
    public abstract class FleetRouteStep : AbstractFleetWaypointProcessStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetMergeStep));

        public FleetRouteStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, TurnGenerationState state)
            : base(gameProvider, rulesProvider, state, WaypointTask.Route)
        {
        }

        /// <summary>
        /// Merge this fleet with another fleet
        /// </summary>
        /// <param name="task"></param>
        /// <returns>true (we message the player if the merge fails, but the waypoint is considered processed)</returns>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            var target = wp.Target as Planet;
            if (target == null)
            {
                // Message.FleetInvalidRouteNotPlanet(player, fleet);
            }
            else if (target is Planet planet)
            {
                if (!player.IsFriend(planet.PlayerNum))
                {
                    // Message.FleetInvalidRouteNotFriendlyPlanet(player, fleet, planet);
                }
                else if (planet.RouteTarget != null)
                {
                    // route!
                    wp.Target = planet.RouteTarget;
                    // Message.FleetRoute(player, fleet, planet, planet.RouteTarget);
                }
                else
                {
                    // Message.FleetInvalidRouteNoRouteTarget(player, fleet, planet);
                }
            }

            return true;
        }

    }
}