using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// This service executes client side fleet orders on the server
    /// </summary>
    public class FleetOrderExecutor
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetOrderExecutor));

        private readonly Game game;
        private readonly FleetService fleetService;
        private readonly CargoTransferer cargoTransferer;

        public FleetOrderExecutor(Game game, FleetService fleetService, CargoTransferer cargoTransferer)
        {
            this.game = game;
            this.fleetService = fleetService;
            this.cargoTransferer = cargoTransferer;
        }

        /// <summary>
        /// The client allows various immediate orders like cargo transfers and merge/split operations.
        /// We process those like WP0 tasks
        /// </summary>
        public void ExecuteFleetOrders(Player player, List<ImmediateFleetOrder> immediateFleetOrders)
        {
            immediateFleetOrders.ForEach(order =>
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

            player.ImmediateFleetOrders.Clear();
            player.CargoTransferOrders.Clear();
            player.MergeFleetOrders.Clear();
            player.SplitFleetOrders.Clear();
        }

        internal void ExecuteCargoTransferOrder(Player player, CargoTransferOrder order)
        {
            if (game.CargoHoldersByGuid.TryGetValue(order.Guid, out var source))
            {
                bool createSalvage = false;
                if (!game.CargoHoldersByGuid.TryGetValue(order.DestGuid, out var dest) && order.Jettison)
                {
                    createSalvage = true;
                    dest = new Salvage() { PlayerNum = player.Num, Guid = order.DestGuid, Position = source.Position };
                }

                if (dest != null)
                {
                    cargoTransferer.Transfer(source, dest, order.Transfer, order.FuelTransfer);

                    // create salvage if this is jettisoned cargo
                    if (createSalvage)
                    {
                        EventManager.PublishMapObjectCreatedEvent(dest as Salvage);
                    }
                }
                else
                {
                    log.Error($"Failed to execute CargoTransferOrder. Destination {order.DestGuid} not found and Jettison not specified.");
                }
            }
        }

        void ExecuteMergeFleetOrder(Player player, MergeFleetOrder order)
        {
            if (game.FleetsByGuid.TryGetValue(order.Guid, out var source))
            {
                if (source.PlayerNum == player.Num)
                {
                    List<Fleet> mergingFleets = new List<Fleet>();
                    foreach (var playerFleetGuid in order.MergingFleetGuids)
                    {
                        if (game.FleetsByGuid.TryGetValue(playerFleetGuid, out var mergingFleet))
                        {
                            if (mergingFleet.PlayerNum == player.Num)
                            {
                                // finally, error checking done, we can merge this one
                                mergingFleets.Add(mergingFleet);
                            }
                            else
                            {
                                log.Error($"Player {player} tried to merge {mergingFleet} into {source.Name}, but they do not own {mergingFleet} - {playerFleetGuid}");
                            }
                        }
                        else
                        {
                            log.Error($"Player {player} tried to merge {mergingFleet} into {source.Name}, but fleet {playerFleetGuid} doesn't exist");
                        }

                    }

                    if (mergingFleets.Count > 0)
                    {
                        fleetService.Merge(source, player, mergingFleets);

                        mergingFleets.ForEach(f => EventManager.PublishMapObjectDeletedEvent(f));
                    }
                }
                else
                {
                    log.Error($"Player {player} tried to merge into a fleet that they don't own: {source.Name} - {source.Guid}");
                }
            }
            else
            {
                log.Error($"Player {player} tried to merge into a fleet that doesn't exist: {order.Guid}");
            }
        }

        void ExecuteSplitAllFleetOrder(Player player, SplitAllFleetOrder order)
        {
            if (game.FleetsByGuid.TryGetValue(order.Guid, out var source))
            {
                if (source.PlayerNum == player.Num)
                {
                    var newFleets = fleetService.Split(source, player, new SplitAllFleetOrder { Guid = source.Guid, NewFleetGuids = order.NewFleetGuids });

                    newFleets.ForEach(f => EventManager.PublishMapObjectCreatedEvent(f));
                    log.Debug($"Executing user SplitAllFleetOrder for {source.Name}");
                }
                else
                {
                    log.Error($"Player {player} tried to split a fleet that they don't own: {source.Name} - {source.Guid}");
                }
            }
            else
            {
                log.Error($"Player {player} tried to split a fleet that doesn't exist: {order.Guid}");
            }

        }

    }
}

