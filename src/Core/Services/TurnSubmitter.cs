using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class TurnSubmitter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TurnSubmitter));

        public Game Game { get; }

        public TurnSubmitter(Game game)
        {
            Game = game;
        }

        public void SubmitTurn(Player player)
        {
            UpdateFleetActions(player);

            player.SubmittedTurn = true;
        }

        /// <summary>
        /// Copy the player's fleet waypoint data into the game's fleet waypoint data
        /// This method makes sure the player client doesnt' try and move the fleet around by updating waypoint 0's position
        /// </summary>
        /// <param name="player"></param>
        internal void UpdateFleetActions(Player player)
        {
            foreach (var playerFleet in player.Fleets.Where(f => f.Player == player))
            {
                if (Game.FleetsByGuid.TryGetValue(playerFleet.Guid, out var fleet) && fleet.Player == player)
                {
                    // Keep waypoint 0 so the client can't move the fleet
                    // remove all the other waypoints for this fleet and replace them with what was sent by the player
                    fleet.Waypoints.RemoveRange(1, fleet.Waypoints.Count - 1);
                    if (playerFleet.Waypoints != null && playerFleet.Waypoints.Count > 0)
                    {
                        // copy player waypoint data, but reset the target/position
                        var wp0 = playerFleet.Waypoints[0];
                        wp0.Target = fleet.Orbiting;
                        if (wp0.Target == null)
                        {
                            wp0.Position = fleet.Position;
                        }
                        fleet.Waypoints[0] = wp0;

                        foreach (var playerWaypoint in playerFleet.Waypoints.Skip(1))
                        {
                            if (playerWaypoint.Target is Planet planet)
                            {
                                if (Game.PlanetsByGuid.TryGetValue(planet.Guid, out var gamePlanet))
                                {
                                    log.Debug($"Adding player defined waypoint for {fleet.Name} to {playerWaypoint.TargetName} -> {playerWaypoint.Task}");
                                    // add the server side version of this planet as a waypoint
                                    fleet.Waypoints.Add(Waypoint.TargetWaypoint(gamePlanet, playerWaypoint.WarpFactor, playerWaypoint.Task, playerWaypoint.TransportTasks));
                                }
                            }
                            else
                            {
                                fleet.Waypoints.Add(playerWaypoint);
                            }
                        };
                    }
                }
            }

            foreach (var playerPlanet in player.Planets.Where(p => p.Player == player))
            {
                if (Game.PlanetsByGuid.TryGetValue(playerPlanet.Guid, out var planet) && planet.Player == player)
                {
                    planet.ContributesOnlyLeftoverToResearch = playerPlanet.ContributesOnlyLeftoverToResearch;
                    // TODO: validate planet production queue
                    planet.ProductionQueue = playerPlanet.ProductionQueue;
                }
            }
        }
    }
}
