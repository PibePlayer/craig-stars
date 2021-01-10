using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class TurnSubmitter
    {
        public Server Server { get; }

        public TurnSubmitter(Server server)
        {
            Server = server;
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
                if (Server.FleetsByGuid.TryGetValue(playerFleet.Guid, out var fleet) && fleet.Player == player)
                {
                    // Keep waypoint 0 so the client can't move the fleet
                    // remove all the other waypoints for this fleet and replace them with what was sent by the player
                    fleet.Waypoints.RemoveRange(1, fleet.Waypoints.Count - 1);
                    if (playerFleet.Waypoints != null && playerFleet.Waypoints.Count > 0)
                    {
                        fleet.Waypoints[0].WarpFactor = playerFleet.Waypoints[0].WarpFactor;
                        fleet.Waypoints[0].Task = playerFleet.Waypoints[0].Task;
                        foreach (var playerWaypoint in playerFleet.Waypoints.Skip(1))
                        {
                            if (playerWaypoint.Target is Planet planet)
                            {
                                if (Server.PlanetsByGuid.TryGetValue(planet.Guid, out var gamePlanet))
                                {
                                    // add the server side version of this planet as a waypoint
                                    fleet.Waypoints.Add(new Waypoint(gamePlanet, playerWaypoint.WarpFactor, playerWaypoint.Task));
                                }
                            }
                            else
                            {
                                fleet.Waypoints.Add(new Waypoint()
                                {
                                    WarpFactor = playerWaypoint.WarpFactor,
                                    Task = playerWaypoint.Task
                                });
                            }
                        };
                    }
                }
            }

            foreach (var playerPlanet in player.Planets.Where(p => p.Player == player))
            {
                if (Server.PlanetsByGuid.TryGetValue(playerPlanet.Guid, out var planet) && planet.Player == player)
                {
                    // TODO: validate planet production queue
                    planet.ProductionQueue.Items.Clear();
                    playerPlanet.ProductionQueue.Items.ForEach(item => planet.ProductionQueue.Items.Add(item.Clone()));
                }
            }
        }
    }
}
