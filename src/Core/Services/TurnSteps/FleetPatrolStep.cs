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
        private readonly FleetDiscoverer fleetDiscoverer;

        public FleetPatrolStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, FleetDiscoverer fleetDiscoverer)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetPatrolStep, WaypointTask.Patrol)
        {
            this.fleetDiscoverer = fleetDiscoverer;
        }

        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            // don't 
            if (fleet.Waypoints.Count > 1)
            {
                return true;
            }

            var distSquared = wp.PatrolRange * wp.PatrolRange;

            var closestDistance = float.MaxValue;
            Fleet closest = null;

            // this step uses player intel. It's almost like a server side turn processor
            foreach (var enemyFleet in player.FleetIntel.Foriegn.Where(fleet => player.IsEnemy(fleet.PlayerNum)))
            {
                var distSquaredToFleet = fleet.Position.DistanceSquaredTo(enemyFleet.Position);
                if (distSquaredToFleet <= distSquared)
                {
                    if (distSquaredToFleet <= closestDistance)
                    {
                        closestDistance = distSquaredToFleet;
                        closest = enemyFleet;
                    }
                }
            }

            if (closest != null)
            {
                int warpFactor = wp.PatrolWarpFactor == Waypoint.PatrolWarpFactorAutomatic ? fleet.Spec.Engine.IdealSpeed : wp.PatrolWarpFactor;
                // we found a target, attack it!
                fleet.Waypoints.Add(Waypoint.TargetWaypoint(Game.FleetsByGuid[closest.Guid], warpFactor));

                // rediscover our fleet so our intel is updated
                // we do this on the server side so a player could leave fleets on patrol and skip a few
                // years, and their attack fleets will maintain a perimeter
                fleetDiscoverer.Discover(player, fleet);

                Message.FleetPatrolTargeted(player, fleet, closest);
            }

            return true;
        }
    }
}