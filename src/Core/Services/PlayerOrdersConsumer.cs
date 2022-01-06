using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class PlayerOrdersConsumer
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerOrdersConsumer));

        private readonly Game game;
        private readonly ImmediateFleetOrderExecutor fleetOrderExecutor;
        private readonly FleetSpecService fleetSpecService;

        public PlayerOrdersConsumer(Game game, ImmediateFleetOrderExecutor fleetOrderExecutor, FleetSpecService fleetSpecService)
        {
            this.game = game;
            this.fleetOrderExecutor = fleetOrderExecutor;
            this.fleetSpecService = fleetSpecService;
        }

        public void ConsumeOrders(PlayerOrders orders)
        {
            var player = game.Players[orders.PlayerNum];

            log.Debug("Processing player immmediate cargo transfers, fleet merges, splits, etc");
            fleetOrderExecutor.ExecuteImmediateFleetOrders(player, orders.ImmedateFleetOrders);

            // save some player config server side
            player.UISettings = orders.UISettings;
            player.Settings = orders.Settings;

            UpdateResearchOrders(player, orders.Research);
            UpdatePlayerRelations(player, orders.PlayerRelations);
            UpdatePlayerPlans(player, orders);

            UpdateFleetWaypointOrders(player, orders.FleetOrders);
            UpdateShipDesigns(player, orders.ShipDesigns);
            UpdateProductionQueues(player, orders.PlanetProductionOrders);
            UpdateMineFields(player, orders.MineFieldOrders);
        }

        private void UpdateResearchOrders(Player player, PlayerResearchOrder research)
        {
            player.Researching = research.Researching;
            player.ResearchAmount = research.ResearchAmount;
            player.NextResearchField = research.NextResearchField;
        }

        private void UpdatePlayerRelations(Player player, List<PlayerRelationship> playerRelations)
        {
            player.PlayerRelations = new(playerRelations);
        }

        private void UpdatePlayerPlans(Player player, PlayerOrders orders)
        {
            player.BattlePlans = new(orders.BattlePlans);
            player.TransportPlans = new(orders.TransportPlans);
            player.ProductionPlans = new(orders.ProductionPlans);
            player.FleetCompositions = new(orders.FleetCompositions);

            player.SetupPlanMappings();
        }

        /// <summary>
        /// Copy the player's ship designs to the game
        /// </summary>
        /// <param name="player"></param>
        void UpdateShipDesigns(Player player, List<ShipDesign> shipDesigns)
        {
            var fleetsToBeDeleted = new List<Fleet>();
            foreach (ShipDesign order in shipDesigns)
            {
                if (order.Status == ShipDesign.DesignStatus.New)
                {
                    // this is a new design
                    log.Debug($"{game.Year}: Adding new Player Design: {player} - {order.Name}.");
                    var newDesign = order.Copy();
                    newDesign.Guid = order.Guid;
                    newDesign.Name = order.Name;
                    game.Designs.Add(newDesign);
                    game.DesignsByGuid[newDesign.Guid] = newDesign;
                }
                else if (game.DesignsByGuid.TryGetValue(order.Guid, out var gameDesign))
                {
                    if (order.Deleted)
                    {
                        // remove this design from player intel so they don't get it back next round
                        var playerDesign = player.DesignsByGuid[order.Guid];
                        player.DesignsByGuid.Remove(order.Guid);
                        player.Designs.Remove(playerDesign);

                        if (gameDesign.InUse)
                        {
                            // delete tokens (and maybe fleets) associated with this design
                            foreach (var fleet in game.Fleets.Where(fleet => fleet.PlayerNum == player.Num).ToList())
                            {
                                var tokenCount = fleet.Tokens.Count;
                                fleet.Tokens = fleet.Tokens.Where(token => token.Design.Guid == order.Guid).ToList();
                                if (fleet.Tokens.Count == 0)
                                {
                                    log.Info($"{game.Year}: Player {player} Deleting fleet with no tokens after deleting design {order.Name}");
                                    EventManager.PublishMapObjectDeletedEvent(fleet);
                                }
                                else if (tokenCount != fleet.Tokens.Count)
                                {
                                    // this fleet lost some tokens, recalc its spec
                                    // TODO: handle cargo and fuel loss as well. This should be replaced
                                    // by a SplitFleetOrder to split out the token and then we can just remove the fleet
                                    fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
                                }
                            }
                        }
                    }
                    else
                    {
                        // update this design
                        gameDesign.Name = order.Name;
                        gameDesign.Version = order.Version;
                        gameDesign.Purpose = order.Purpose;
                        gameDesign.HullSetNumber = order.HullSetNumber;
                        gameDesign.Slots = new(order.Slots);
                    }
                }
            }
        }

        /// <summary>
        /// Copy the player's fleet waypoint data into the game's fleet waypoint data
        /// This method makes sure the player client doesnt' try and move the fleet around by updating waypoint 0's position
        /// </summary>
        /// <param name="player"></param>
        internal void UpdateFleetWaypointOrders(Player player, List<FleetWaypointsOrders> orders)
        {
            orders.ForEach(order =>
            {
                // update the game fleet
                var fleet = game.FleetsByGuid[order.Guid];

                // handle renames, battle plan changes, and clicking the repeat orders checkbox
                fleet.BaseName = order.BaseName;
                fleet.BattlePlan = player.BattlePlansByGuid[order.BattlePlanGuid];
                fleet.RepeatOrders = order.RepeatOrders;

                // start with an empty wp0
                fleet.Waypoints = new List<Waypoint>() {
                    Waypoint.PositionWaypoint(fleet.Position)
                };

                // add in each fleet waypoint
                order.Waypoints.Each((orderWp, index) =>
                {
                    // if this is waypoint 0, use our empty waypoint 0 with position data
                    // otherwise copy the position from the order waypoint. We'll update targets later
                    var wp = index == 0 ? fleet.Waypoints[0] : new Waypoint() { Position = orderWp.Position.Round() };

                    // copy the orders from the orders waypoint
                    wp.OriginalPosition = orderWp.OriginalPosition;
                    wp.Task = orderWp.Task;
                    wp.WarpFactor = orderWp.WarpFactor;
                    wp.TransportTasks = orderWp.TransportTasks;
                    wp.LayMineFieldDuration = orderWp.LayMineFieldDuration;
                    wp.PatrolRange = orderWp.PatrolRange;
                    wp.PatrolWarpFactor = orderWp.PatrolWarpFactor;
                    wp.TransferToPlayer = orderWp.TransferToPlayer;

                    // update targets
                    if (orderWp.TargetGuid != null && game.MapObjectsByGuid.TryGetValue(orderWp.TargetGuid.Value, out var target))
                    {
                        wp.Target = target;
                    }
                    if (orderWp.OriginalTargetGuid != null && game.MapObjectsByGuid.TryGetValue(orderWp.OriginalTargetGuid.Value, out var originalTarget))
                    {
                        wp.OriginalTarget = originalTarget;
                    }

                    if (index > 0)
                    {
                        fleet.Waypoints.Add(wp);
                    }
                });
            });
        }

        void UpdateProductionQueues(Player player, List<PlanetProductionOrder> planetProductionOrders)
        {
            planetProductionOrders.ForEach(order =>
            {
                var planet = game.PlanetsByGuid[order.Guid];
                planet.ContributesOnlyLeftoverToResearch = order.ContributesOnlyLeftoverToResearch;
                planet.ProductionQueue.Items.Clear();
                planet.ProductionQueue.Items.AddRange(order.Items.Select(item => item.Clone()).Select(item =>
                {
                    item.Design = item.DesignGuid.HasValue ? game.DesignsByGuid[item.DesignGuid.Value] : null;
                    return item;
                }));

                planet.PacketSpeed = order.PacketSpeed;
                planet.PacketTarget = order.PacketTarget.HasValue ? game.MapObjectsByGuid[order.PacketTarget.Value] : null;
                planet.RouteTarget = order.RouteTarget.HasValue ? game.MapObjectsByGuid[order.RouteTarget.Value] : null;
                if (order.StarbaseBattlePlanGuid.HasValue)
                {
                    planet.Starbase.BattlePlan = player.BattlePlansByGuid[order.StarbaseBattlePlanGuid.Value];
                }
            });
        }

        /// <summary>
        /// Enable detonation
        /// </summary>
        /// <param name="player"></param>
        void UpdateMineFields(Player player, List<MineFieldOrder> mineFieldOrders)
        {
            mineFieldOrders.ForEach(order =>
            {
                game.MineFieldsByGuid[order.Guid].Detonate = order.Detonate;
            });
        }
    }
}
