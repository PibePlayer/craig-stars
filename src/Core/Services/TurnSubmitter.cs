using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class TurnSubmitter
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnSubmitter));

        private readonly Game game;
        private readonly FleetOrderExecutor fleetOrderExecutor;

        public TurnSubmitter(Game game, FleetOrderExecutor fleetOrderExecutor)
        {
            this.game = game;
            this.fleetOrderExecutor = fleetOrderExecutor;
        }

        public void SubmitTurn(Player player)
        {
            log.Debug("Processing player immmediate cargo transfers, fleet merges, splits, etc");
            fleetOrderExecutor.ExecuteFleetOrders(player);

            UpdateFleetActions(player);
            UpdateShipDesigns(player);
            UpdateProductionQueues(player);
            UpdateMineFields(player);

            game.Players[player.Num].SubmittedTurn = true;
            player.SubmittedTurn = true;
        }

        /// <summary>
        /// Copy the player's ship designs to the game
        /// </summary>
        /// <param name="player"></param>
        void UpdateShipDesigns(Player player)
        {
            foreach (var playerDesign in player.Designs)
            {
                if (game.DesignsByGuid.TryGetValue(playerDesign.Guid, out var design))
                {
                    if (design.PlayerNum != player.Num)
                    {
                        log.Error($"{game.Year}: Player {player} is trying to update design {design.Name}, owned by {design.PlayerNum}");
                        continue;
                    }
                    // see if this design can be updated
                    if (design.InUse)
                    {
                        log.Debug($"{game.Year}: Not updating Player Design: {player} - {design.Name}. It is in use");
                    }
                    else
                    {
                        // TODO: update design name, slots, etc
                        log.Debug($"{game.Year}: Updating Game design from Player Design: {player} - {design.Name}.");
                    }
                }
                else
                {
                    // this is a new design
                    log.Debug($"{game.Year}: Adding new Player Design: {player} - {playerDesign.Name}.");
                    var newDesign = playerDesign.Copy();
                    newDesign.Name = playerDesign.Name;
                    newDesign.Guid = playerDesign.Guid;
                    game.Designs.Add(newDesign);
                    game.DesignsByGuid[newDesign.Guid] = newDesign;
                }
            }
            foreach (var design in game.Designs.Where(d => d.PlayerNum == player.Num).ToList())
            {
                if (!player.DesignsByGuid.ContainsKey(design.Guid))
                {
                    // the player doesn't have this design
                    log.Warn($"{game.Year}: Player {player} no longer has design that is present in Game designs {design.Name}");
                }
            }
        }

        /// <summary>
        /// Copy the player's fleet waypoint data into the game's fleet waypoint data
        /// This method makes sure the player client doesnt' try and move the fleet around by updating waypoint 0's position
        /// </summary>
        /// <param name="player"></param>
        internal void UpdateFleetActions(Player player)
        {
            foreach (var playerFleet in player.Fleets)
            {
                log.Debug($"{game.Year}: Updating Fleet Actions for {playerFleet.PlayerNum} - {playerFleet.Name}");
                if (game.FleetsByGuid.TryGetValue(playerFleet.Guid, out var fleet) && fleet.PlayerNum == player.Num)
                {
                    fleet.BattlePlan = playerFleet.BattlePlan.Clone();
                    fleet.RepeatOrders = playerFleet.RepeatOrders;
                    // Keep waypoint 0 so the client can't move the fleet
                    // remove all the other waypoints for this fleet and replace them with what was sent by the player
                    fleet.Waypoints.RemoveRange(1, fleet.Waypoints.Count - 1);
                    if (playerFleet.Waypoints != null && playerFleet.Waypoints.Count > 0)
                    {
                        // copy player waypoint data, but reset the target/position
                        var wp0 = fleet.Waypoints[0];
                        wp0.Target = fleet.Orbiting;
                        wp0.OriginalTarget = fleet.Waypoints[0].OriginalTarget;
                        wp0.OriginalPosition = fleet.Waypoints[0].OriginalPosition;
                        if (wp0.Target == null)
                        {
                            wp0.Position = fleet.Position;
                        }
                        var playerWp0 = playerFleet.Waypoints[0];
                        if (wp0.Task != playerWp0.Task)
                        {
                            log.Debug($"{game.Year}: Updating waypoint task for {fleet.Name} to {playerWp0.TargetName} -> {playerWp0.Task}");
                        }
                        wp0.Task = playerWp0.Task;
                        wp0.WarpFactor = playerWp0.WarpFactor;
                        wp0.TransportTasks = playerWp0.TransportTasks;
                        wp0.LayMineFieldDuration = playerWp0.LayMineFieldDuration;

                        var index = 1;

                        foreach (var playerWaypoint in playerFleet.Waypoints.Skip(1))
                        {
                            if (playerWaypoint.Target != null && playerWaypoint.Target == playerFleet.Waypoints[index - 1].Target)
                            {
                                // don't let the client submit multiple waypoints to the same location in a row
                                log.Error($"{game.Year}: Player fleet {playerFleet.Name} tried to create two waypoints (wp{index - 1} and wp{index}) in a row targetting {playerWaypoint.Target}.");
                                continue;
                            }
                            if (playerWaypoint.Target is MapObject mapObject)
                            {
                                if (game.MapObjectsByGuid.TryGetValue(mapObject.Guid, out var gameMapObject))
                                {
                                    log.Debug($"{game.Year}: Adding player defined waypoint for {fleet.Name} to {playerWaypoint.TargetName} -> Task: {playerWaypoint.Task}");
                                    // add the server side version of this planet as a waypoint
                                    fleet.Waypoints.Add(Waypoint.TargetWaypoint(gameMapObject, playerWaypoint.WarpFactor, playerWaypoint.Task, playerWaypoint.TransportTasks, playerWaypoint.LayMineFieldDuration, playerWaypoint.PatrolRange, playerWaypoint.PatrolWarpFactor, playerWaypoint.TransferToPlayer));
                                }
                                else
                                {
                                    log.Error($"{game.Year}: Player waypoint Target {mapObject} was not found in Game MapObjects.");
                                }
                            }
                            else
                            {
                                fleet.Waypoints.Add(playerWaypoint.Clone());
                            }

                            // make sure the original target maps to a game object
                            if (playerWaypoint.OriginalTarget is MapObject originalTarget)
                            {
                                if (game.MapObjectsByGuid.TryGetValue(originalTarget.Guid, out var gameOriginalTarget))
                                {
                                    fleet.Waypoints[fleet.Waypoints.Count - 1].OriginalTarget = gameOriginalTarget;
                                }
                                else
                                {
                                    log.Error($"{game.Year}: Player waypoint OriginalTarget {originalTarget} was not found in Game MapObjects.");
                                }
                            }
                            fleet.Waypoints[fleet.Waypoints.Count - 1].OriginalPosition = playerWaypoint.OriginalPosition;
                            index++;
                        };
                    }
                }
                else
                {
                    log.Error($"{game.Year}: Could not find Game Fleet for Player Fleet: {playerFleet.Name}, Guid: {playerFleet.Guid}");
                }
            }

        }

        void UpdateProductionQueues(Player player)
        {
            foreach (var playerPlanet in player.Planets)
            {
                if (game.PlanetsByGuid.TryGetValue(playerPlanet.Guid, out var planet) && planet.PlayerNum == player.Num)
                {
                    planet.ContributesOnlyLeftoverToResearch = playerPlanet.ContributesOnlyLeftoverToResearch;
                    // copy each production queue item from the player planet
                    // replacing the player's design with our game design

                    planet.ProductionQueue.Items.Clear();
                    planet.ProductionQueue.Items.AddRange(playerPlanet.ProductionQueue.Items.Select(item => item.Clone()).Select(item =>
                    {
                        if (item.Design != null)
                        {
                            if (game.DesignsByGuid.TryGetValue(item.Design.Guid, out var design))
                            {
                                // use the Game design, not the player one
                                item.Design = design;
                            }
                            else
                            {
                                log.Error($"{game.Year}: Player ProductionQueueItem has unknown design: {player} - {design.Name}");
                            }
                        }
                        return item;
                    }));

                    // update starbase battleplan
                    if (playerPlanet.HasStarbase && planet.HasStarbase)
                    {
                        planet.Starbase.BattlePlan = playerPlanet.Starbase.BattlePlan;
                    }

                    // update packet target
                    planet.PacketSpeed = playerPlanet.PacketSpeed;
                    planet.PacketTarget = null;
                    if (playerPlanet.PacketTarget != null && game.PlanetsByGuid.TryGetValue(playerPlanet.PacketTarget.Guid, out var target))
                    {
                        planet.PacketTarget = target;
                    }
                }
            }
        }

        /// <summary>
        /// Enable detonation
        /// </summary>
        /// <param name="player"></param>
        void UpdateMineFields(Player player)
        {
            foreach (var playerMineField in player.MineFields)
            {
                if (game.MineFieldsByGuid.TryGetValue(playerMineField.Guid, out var mineField))
                {
                    if (mineField.OwnedBy(player))
                    {
                        if (player.Race.Spec.CanDetonateMineFields)
                        {
                            mineField.Detonate = playerMineField.Detonate;
                        }
                    }
                    else
                    {
                        log.Error($"{game.Year}: Game MineField not owned by player MineField: {playerMineField.Name}, Guid: {playerMineField.Guid}, Player: {player}");
                    }
                }
                else
                {
                    log.Error($"{game.Year}: Could not find Game MineField for Player MineField: {playerMineField.Name}, Guid: {playerMineField.Guid}");
                }
            }
        }


    }
}
