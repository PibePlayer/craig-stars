using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// This service executes client side fleet orders on the server
    /// </summary>
    public class FleetOrderExecutor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetOrderExecutor));

        public Game Game { get; }

        public FleetOrderExecutor(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// The client allows various immediate orders like cargo transfers and merge/split operations.
        /// We process those like WP0 tasks
        /// </summary>
        public void ExecuteFleetOrders(Player player)
        {
            player.FleetOrders.ForEach(order =>
            {
                if (order is CargoTransferOrder cargoTransferOrder)
                {
                    ExecuteCargoTransferOrder(player, cargoTransferOrder);
                }
                else if (order is MergeFleetOrder mergeFleetOrder)
                {
                    ExecuteMergeFleetOrder(player, mergeFleetOrder);
                }
                else if (order is SplitAllFleetOrder splitAllFleetOrder)
                {
                    ExecuteSplitAllFleetOrder(player, splitAllFleetOrder);
                }
            });

            player.FleetOrders.Clear();
            player.CargoTransferOrders.Clear();
            player.MergeFleetOrders.Clear();
            player.SplitFleetOrders.Clear();
        }

        void ExecuteCargoTransferOrder(Player player, CargoTransferOrder order)
        {
            if (Game.CargoHoldersByGuid.TryGetValue(order.Source.Guid, out var source) &&
            Game.CargoHoldersByGuid.TryGetValue(order.Dest.Guid, out var dest))
            {
                // make sure our source can lose the cargo
                var result = source.AttemptTransfer(order.Transfer, order.FuelTransfer);
                if (result)
                {
                    // make sure our dest can take the cargo
                    result = dest.AttemptTransfer(-order.Transfer, order.FuelTransfer);
                    if (!result)
                    {
                        // revert the source changes
                        source.Cargo -= order.Transfer;
                        log.Error($"Player {player} Failed to transfer {order.Transfer} from {source.Name} to {dest.Name}. {dest.Name} rejected cargo.");
                    }
                }
                else
                {
                    log.Error($"Player {player} Failed to transfer {order.Transfer} from {source.Name} to {dest.Name}. {source.Name} rejected cargo.");
                }
            }
        }

        void ExecuteMergeFleetOrder(Player player, MergeFleetOrder order)
        {
            if (Game.FleetsByGuid.TryGetValue(order.Source.Guid, out var source))
            {
                if (source.Player == player)
                {
                    List<Fleet> mergingFleets = new List<Fleet>();
                    foreach (var playerFleet in order.MergingFleets)
                    {
                        if (Game.FleetsByGuid.TryGetValue(playerFleet.Guid, out var mergingFleet))
                        {
                            if (mergingFleet.Player == player)
                            {
                                // finally, error checking done, we can merge this one
                                mergingFleets.Add(mergingFleet);
                            }
                            else
                            {
                                log.Error($"Player {player} tried to merge {playerFleet.Name} into {source.Name}, but they do not own {playerFleet.Name} - {playerFleet.Guid}");
                            }
                        }
                        else
                        {
                            log.Error($"Player {player} tried to merge {playerFleet.Name} into {source.Name}, but {playerFleet.Name} - {playerFleet.Guid} doesn't exist");
                        }

                    }

                    if (mergingFleets.Count > 0)
                    {
                        source.Merge(new MergeFleetOrder()
                        {
                            Source = source,
                            MergingFleets = mergingFleets
                        });

                        mergingFleets.ForEach(f => EventManager.PublishFleetDeletedEvent(f));
                    }
                }
                else
                {
                    log.Error($"Player {player} tried to merge into a fleet that they don't own: {order.Source.Name} - {order.Source.Guid}");
                }
            }
            else
            {
                log.Error($"Player {player} tried to merge into a fleet that doesn't exist: {order.Source.Name} - {order.Source.Guid}");
            }
        }

        void ExecuteSplitAllFleetOrder(Player player, SplitAllFleetOrder order)
        {
            if (Game.FleetsByGuid.TryGetValue(order.Source.Guid, out var source))
            {
                if (source.Player == player)
                {
                    var newFleets = source.Split(new SplitAllFleetOrder { Source = source, NewFleetGuids = order.NewFleetGuids });

                    newFleets.ForEach(f => EventManager.PublishFleetCreatedEvent(f));
                    log.Debug($"Executing user SplitAllFleetOrder for {source.Name}");
                }
                else
                {
                    log.Error($"Player {player} tried to split a fleet that they don't own: {order.Source.Name} - {order.Source.Guid}");
                }
            }
            else
            {
                log.Error($"Player {player} tried to split a fleet that doesn't exist: {order.Source.Name} - {order.Source.Guid}");
            }

        }

    }
}

