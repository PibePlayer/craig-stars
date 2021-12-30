using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;

namespace CraigStars
{
    public class PlayerOrdersValidatorResult
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerOrdersValidator));

        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
            log.Error(error);
        }

        public void AddWarning(string error)
        {
            Warnings.Add(error);
            log.Warn(error);
        }

    }

    public class PlayerOrdersValidatorContext
    {
        public Dictionary<Guid, MapObject> MapObjectsByGuid { get; }
        public Dictionary<Guid, ICargoHolder> CargoHoldersByGuid { get; }
        public Dictionary<Guid, Fleet> FleetsByGuid { get; }
        public Dictionary<Guid, ShipDesign> DesignsByGuid { get; }

        public Dictionary<Guid, BattlePlan> BattlePlansByGuid { get; set; }
        public Dictionary<Guid, TransportPlan> TransportPlansByGuid { get; set; }
        public Dictionary<Guid, ProductionPlan> ProductionPlansByGuid { get; set; }

        public PlayerOrdersValidatorContext(Game game, Player player)
        {
            MapObjectsByGuid = new(game.MapObjectsByGuid);
            FleetsByGuid = new(game.FleetsByGuid);
            CargoHoldersByGuid = new(game.CargoHoldersByGuid);

            BattlePlansByGuid = new(player.BattlePlansByGuid);
            TransportPlansByGuid = new(player.TransportPlansByGuid);
            ProductionPlansByGuid = new(player.ProductionPlansByGuid);
            DesignsByGuid = new(player.DesignsByGuid);
        }
    }

    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class PlayerOrdersValidator
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerOrdersValidator));

        private readonly Game game;

        public PlayerOrdersValidator(Game game)
        {
            this.game = game;
        }

        public PlayerOrdersValidatorResult Validate(PlayerOrders orders)
        {
            var result = new PlayerOrdersValidatorResult();

            // first validate the player and token
            ValidatePlayerToken(orders, result);

            if (result.IsValid)
            {
                // create context for the rest of the validations
                var player = game.Players[orders.PlayerNum];
                var context = new PlayerOrdersValidatorContext(game, player);

                context.BattlePlansByGuid = orders.BattlePlans.ToGuidDictionary(_ => _.Guid);
                context.TransportPlansByGuid = orders.TransportPlans.ToGuidDictionary(_ => _.Guid);
                context.ProductionPlansByGuid = orders.ProductionPlans.ToGuidDictionary(_ => _.Guid);

                ValidatePlayerRelations(orders, result);
                ValidatePlayerResearch(orders, result);

                // validate fleets, both immediate transfers, merges, splits and then
                // regular waypoint orders
                ValidateImmediateFleetOrders(orders, context, result);
                ValidateFleetOrders(orders, context, result);

                ValidateDesigns(orders, context, result);
                ValidatePlanetProductionOrders(orders, context, result);
                ValidateMineFields(orders, context, result);
            }

            return result;
        }

        /// <summary>
        /// Validate the player's number and token
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal void ValidatePlayerToken(PlayerOrders orders, PlayerOrdersValidatorResult result)
        {
            if (orders.PlayerNum < 0 || orders.PlayerNum > game.Players.Count)
            {
                result.AddError($"{game.Name}:{game.Year} Player Num is out of range for {game.Players.Count} players.");
            }
            else if (game.Players[orders.PlayerNum].Token != orders.Token)
            {
                result.AddError($"{game.Name}:{game.Year} Player Token is invalid.");
            }
        }

        /// <summary>
        /// Validate the player's number and token
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal void ValidatePlayerRelations(PlayerOrders orders, PlayerOrdersValidatorResult result)
        {
            if (game.Players.Count > orders.PlayerRelations.Count)
            {
                result.AddError($"{game.Name}:{game.Year} Has more players than the orders have PlayerRelations");
            }
        }

        /// <summary>
        /// Validate that research settings work
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="result"></param>
        internal void ValidatePlayerResearch(PlayerOrders orders, PlayerOrdersValidatorResult result)
        {
            if (orders.Research.ResearchAmount < 0 || orders.Research.ResearchAmount > 100)
            {
                result.AddError($"{game.Name}:{game.Year} Research is {orders.Research} but must be between 0 and 100");
            }
        }

        /// <summary>
        /// Validate the fleet's immediate fleet orders. Update the fleet context with any new or old fleet merges
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        internal void ValidateImmediateFleetOrders(PlayerOrders orders, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            var player = game.Players[orders.PlayerNum];

            orders.ImmedateFleetOrders.ForEach(order =>
            {
                if (order is CargoTransferOrder cargoTransferOrder)
                {
                    ValidateCargoTransferOrder(cargoTransferOrder, context, result);
                }
                else if (order is MergeFleetOrder mergeFleetOrder)
                {
                    ValidateMergeFleetOrder(player, context, mergeFleetOrder, result);
                }
                else if (order is SplitAllFleetOrder splitAllFleetOrder)
                {
                    ValidateSplitAllFleetOrder(player, context, splitAllFleetOrder, result);
                }
            });
        }

        void ValidateCargoTransferOrder(CargoTransferOrder order, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            if (!context.CargoHoldersByGuid.TryGetValue(order.Guid, out var source))
            {
                result.AddError($"{game.Name}:{game.Year} CargoTransferOrder source {order.Guid} was not found in game ICargoHolders");
            }
            if (!context.CargoHoldersByGuid.TryGetValue(order.DestGuid, out var dest))
            {
                result.AddError($"{game.Name}:{game.Year} CargoTransferOrder dest {order.DestGuid} was not found in game ICargoHolders");
            }
        }

        void ValidateMergeFleetOrder(Player player, PlayerOrdersValidatorContext context, MergeFleetOrder order, PlayerOrdersValidatorResult result)
        {
            if (context.FleetsByGuid.TryGetValue(order.Guid, out var source))
            {
                if (source.PlayerNum == player.Num)
                {
                    List<Fleet> mergingFleets = new List<Fleet>();
                    foreach (var playerFleetGuid in order.MergingFleetGuids)
                    {
                        if (context.FleetsByGuid.TryGetValue(playerFleetGuid, out var mergingFleet))
                        {
                            if (mergingFleet.PlayerNum == player.Num)
                            {
                                // finally, error checking done, we can merge this one
                                mergingFleets.Add(mergingFleet);
                            }
                            else
                            {
                                result.AddError($"Player {player} tried to merge {mergingFleet} into {source.Name}, but they do not own {mergingFleet} - {playerFleetGuid}");
                            }
                        }
                        else
                        {
                            result.AddError($"Player {player} tried to merge fleet {playerFleetGuid} into {source.Name}, but fleet {playerFleetGuid} doesn't exist");
                        }

                    }

                    if (mergingFleets.Count > 0)
                    {
                        // remove the merging fleets from our maps so validation will fail if we try and transfer cargo or something
                        // from a fleet that was merged into another
                        mergingFleets.ForEach(fleet =>
                        {
                            context.MapObjectsByGuid.Remove(fleet.Guid);
                            context.FleetsByGuid.Remove(fleet.Guid);
                            context.CargoHoldersByGuid.Remove(fleet.Guid);
                        });
                    }
                }
                else
                {
                    result.AddError($"Player {player} tried to merge into a fleet that they don't own: {source.Name} - {order.Guid}");
                }
            }
            else
            {
                result.AddError($"Player {player} tried to merge into a fleet that doesn't exist: {order.Guid}");
            }
        }

        void ValidateSplitAllFleetOrder(Player player, PlayerOrdersValidatorContext context, SplitAllFleetOrder order, PlayerOrdersValidatorResult result)
        {
            if (context.FleetsByGuid.TryGetValue(order.Guid, out var source))
            {
                if (source.PlayerNum == player.Num)
                {
                    foreach (var newFleet in order.NewFleetGuids.Select(guid => new Fleet() { Guid = guid, PlayerNum = player.Num, Position = source.Position }))
                    {
                        context.FleetsByGuid[newFleet.Guid] = newFleet;
                        context.MapObjectsByGuid[newFleet.Guid] = newFleet;
                        context.CargoHoldersByGuid[newFleet.Guid] = newFleet;
                    }
                    log.Debug($"Adding new fleets for validation from SplitAllFleetOrder for {source.Name}");
                }
                else
                {
                    result.AddError($"Player {player} tried to split a fleet that they don't own: {source.Name} - {order.Guid}");
                }
            }
            else
            {
                result.AddError($"Player {player} tried to split a fleet that doesn't exist: {order.Guid}");
            }

        }

        /// <summary>
        /// Validate that the fleet's waypoints point to valid targets in the context.
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="result"></param>
        internal void ValidateFleetOrders(PlayerOrders orders, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            foreach (var order in orders.FleetOrders)
            {
                if (context.FleetsByGuid.TryGetValue(order.Guid, out var fleet))
                {
                    // check waypoints
                    order.Waypoints.Each((wp, index) =>
                    {
                        if (wp.TargetGuid != null)
                        {
                            if (!context.MapObjectsByGuid.TryGetValue(wp.TargetGuid.Value, out var targetMapObject))
                            {
                                // it's possible this target was scrapped, so revisit this...
                                // for now we do a TryGetValue on the consumer, so it shouldn't fail
                                // result.AddError($"{game.Name}:{game.Year} No MapObject found for waypoint target {wp.TargetGuid} - {wp.TargetName}");
                            }
                            else
                            {
                                // make sure the wp0 target is actually at the same location as the fleet
                                if (index == 0 && targetMapObject.Position != fleet.Position)
                                {
                                    // this is just a warning
                                    result.AddWarning($"{game.Name}:{game.Year} {fleet.Name} wp0 Target {targetMapObject.Name} is at {targetMapObject.Position} but fleet is at {fleet.Position}");
                                }
                            }
                        }
                        if (wp.OriginalTargetGuid != null)
                        {
                            if (!context.MapObjectsByGuid.TryGetValue(wp.OriginalTargetGuid.Value, out var targetMapObject))
                            {
                                result.AddWarning($"{game.Name}:{game.Year} No MapObject found for waypoint original target {wp.OriginalTargetGuid}");
                            }
                        }
                    });
                }
                else
                {
                    result.AddError($"{game.Name}:{game.Year} No fleet found for guid {order.Guid}");
                }
            }
        }

        /// <summary>
        /// Validate the player's ship designs
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        internal void ValidateDesigns(PlayerOrders orders, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            foreach (var design in orders.ShipDesigns)
            {
                if (design.Deleted)
                {
                    context.DesignsByGuid.Remove(design.Guid);
                }
                else if (design.Status == ShipDesign.DesignStatus.New)
                {
                    // check this design for validity (i.e. it has all required slots)
                    if (design.IsValid())
                    {
                        // make sure this new design is in our context, in case it's new
                        context.DesignsByGuid[design.Guid] = design;
                    }
                    else
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} design {design.Name} is invalid.");
                    }
                }
                else if (context.DesignsByGuid.TryGetValue(design.Guid, out var gameDesign))
                {
                    // make sure the player can modify this design
                    if (gameDesign.PlayerNum != orders.PlayerNum)
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} is trying to edit design {gameDesign.Name} that belongs to Player {gameDesign.PlayerNum}.");
                    }
                    else
                    {
                        // check this design for validity (i.e. it has all required slots)
                        if (design.IsValid())
                        {
                            // make sure this new design is in our context, in case it's new
                            context.DesignsByGuid[design.Guid] = design;
                        }
                        else
                        {
                            result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} design {design.Name} is invalid.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate orders for planets
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        private void ValidatePlanetProductionOrders(PlayerOrders orders, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            var player = game.Players[orders.PlayerNum];
            orders.PlanetProductionOrders.ForEach(order =>
            {
                // validate planet exists and is owned by player
                if (context.MapObjectsByGuid.TryGetValue(order.Guid, out var source) && source is Planet planet)
                {
                    // validate planet ownership
                    if (!planet.OwnedBy(orders.PlayerNum))
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has orders but is not owned by the Player.");
                    }

                    // validate RouteTarget
                    if (order.PacketTarget.HasValue && !context.MapObjectsByGuid.TryGetValue(order.PacketTarget.Value, out var packetTarget))
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has PacketTarget {order.PacketTarget} that was not found in game.");
                    }

                    // validate PacketTarget
                    if (order.RouteTarget.HasValue && !context.MapObjectsByGuid.TryGetValue(order.RouteTarget.Value, out var routeTarget))
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has RouteTarget {order.RouteTarget} that was not found in game.");
                    }

                    // validate StarbaseBattlePlan
                    if (order.StarbaseBattlePlanGuid.HasValue)
                    {
                        if (!planet.HasStarbase)
                        {
                            result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has BattlePlan but planet has no Starbase");
                        }
                        if (!context.BattlePlansByGuid.TryGetValue(order.StarbaseBattlePlanGuid.Value, out var battlePlan))
                        {
                            result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has BattlePlan {order.StarbaseBattlePlanGuid} that was not found in Player BattlePlans.");
                        }
                    }

                    foreach (var item in order.Items)
                    {
                        if (item.DesignGuid.HasValue && !context.DesignsByGuid.TryGetValue(item.DesignGuid.Value, out var design))
                        {
                            result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet {planet.Name} has unknown design ({item.DesignGuid}) in ProductionQueue");
                        }
                    }
                }
                else
                {
                    result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} planet order {order.Guid} not found in game Planets.");
                }
            });
        }

        internal void ValidateMineFields(PlayerOrders orders, PlayerOrdersValidatorContext context, PlayerOrdersValidatorResult result)
        {
            foreach (MineFieldOrder order in orders.MineFieldOrders)
            {
                if (context.MapObjectsByGuid.TryGetValue(order.Guid, out var mapObject) && mapObject is MineField mineField)
                {
                    if (!mineField.OwnedBy(orders.PlayerNum))
                    {
                        result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} minefield {mineField.Name} has orders but is not owned by the Player.");
                    }
                }
                else
                {
                    result.AddError($"{game.Name}:{game.Year} Player {orders.PlayerNum} tried to command a MineField that doesn't exist: {order.Guid}");
                }
            }
        }

    }
}
