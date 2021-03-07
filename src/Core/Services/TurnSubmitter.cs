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
            UpdateShipDesigns(player);
            UpdateProductionQueues(player);

            player.SubmittedTurn = true;
        }

        /// <summary>
        /// Copy the player's ship designs to the game
        /// </summary>
        /// <param name="player"></param>
        void UpdateShipDesigns(Player player)
        {
            foreach (var playerDesign in player.Designs.Where(d => d.Player == player))
            {
                if (Game.DesignsByGuid.TryGetValue(playerDesign.Guid, out var design))
                {
                    if (design.Player != player)
                    {
                        log.Error($"Player {player} is trying to update design {design.Name}, owned by {design.Player}");
                        continue;
                    }
                    // see if this design can be updated
                    if (design.Aggregate.InUse)
                    {
                        log.Debug($"Not updating Player Design: {player} - {design.Name}. It is in use");
                    }
                    // TODO: update design name, slots, etc
                }
                else
                {
                    // this is a new design
                    log.Debug($"Adding new Player Design: {player} - {playerDesign.Name}.");
                    var newDesign = playerDesign.Copy();
                    newDesign.Name = playerDesign.Name;
                    newDesign.Guid = playerDesign.Guid;
                    Game.Designs.Add(newDesign);
                    Game.DesignsByGuid[newDesign.Guid] = newDesign;
                }
            }
            foreach (var design in Game.Designs.Where(d => d.Player == player))
            {
                if (!player.DesignsByGuid.ContainsKey(design.Guid))
                {
                    // the player doesn't have this design

                }
            }
        }

        /// <summary>
        /// Copy the player's fleet waypoint data into the game's fleet waypoint data
        /// This method makes sure the player client doesnt' try and move the fleet around by updating waypoint 0's position
        /// </summary>
        /// <param name="player"></param>
        void UpdateFleetActions(Player player)
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
                        var wp0 = fleet.Waypoints[0];
                        wp0.Target = fleet.Orbiting;
                        if (wp0.Target == null)
                        {
                            wp0.Position = fleet.Position;
                        }
                        var playerWp0 = playerFleet.Waypoints[0];
                        wp0.Task = playerWp0.Task;
                        wp0.WarpFactor = playerWp0.WarpFactor;
                        wp0.TransportTasks = playerWp0.TransportTasks;

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
                else
                {
                    log.Error($"Could not find Game Fleet for Player Fleet: {playerFleet.Name}, Guid: {playerFleet.Guid}");
                }
            }

        }

        void UpdateProductionQueues(Player player)
        {
            foreach (var playerPlanet in player.Planets.Where(p => p.Player == player))
            {
                if (Game.PlanetsByGuid.TryGetValue(playerPlanet.Guid, out var planet) && planet.Player == player)
                {
                    planet.ContributesOnlyLeftoverToResearch = playerPlanet.ContributesOnlyLeftoverToResearch;
                    // copy each production queue item from the player planet
                    // replacing the player's design with our game design

                    planet.ProductionQueue.Items.Clear();
                    planet.ProductionQueue.Items.AddRange(playerPlanet.ProductionQueue.Items);
                    planet.ProductionQueue.Items.ForEach(item =>
                    {
                        if (item.Design != null)
                        {
                            if (Game.DesignsByGuid.TryGetValue(item.Design.Guid, out var design))
                            {
                                // use the Game design, not the player one
                                item.Design = design;
                            }
                            else
                            {
                                log.Error($"Player ProductionQueueItem has unknown design: {player} - {design.Name}");
                            }
                        }
                    });
                }
            }
        }

    }
}
