using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetUnload0Step : FleetUnloadStep
    {
        public FleetUnload0Step(
                IProvider<Game> gameProvider,
                IRulesProvider rulesProvider,
                InvasionProcessor invasionProcessor
            ) : base(gameProvider, rulesProvider, invasionProcessor, TurnGenerationState.FleetUnload0Step) { }
    }

    public class FleetUnload1Step : FleetUnloadStep
    {
        public FleetUnload1Step(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            InvasionProcessor invasionProcessor
        ) : base(gameProvider, rulesProvider, invasionProcessor, TurnGenerationState.FleetUnload1Step) { }
    }

    /// <summary>
    /// Process all unload cargo tasks
    /// </summary>
    public abstract class FleetUnloadStep : AbstractFleetTransportStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetUnloadStep));

        public FleetUnloadStep(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            InvasionProcessor invasionProcessor,
            TurnGenerationState state
            ) : base(gameProvider, rulesProvider, invasionProcessor, state) { }

        /// <summary>
        /// Process any unload tasks
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (source, wp, player) = task;

            if (wp.Target is ICargoHolder dest)
            {
                Cargo cargoToUnload;
                foreach (var transportTask in task.TransportTasks)
                {
                    int availableToUnload = transportTask.cargoType == CargoType.Fuel ? source.Fuel : source.Cargo[transportTask.cargoType];
                    int transferAmount = 0;
                    switch (transportTask.action)
                    {
                        case WaypointTaskTransportAction.UnloadAll:
                            // unload all available, based on our constraints
                            transferAmount = Math.Min(availableToUnload, dest.AvailableCapacity);
                            break;
                        case WaypointTaskTransportAction.UnloadAmount:
                            // don't unload more than the task says
                            transferAmount = Math.Min(Math.Min(availableToUnload, transportTask.amount), dest.AvailableCapacity);
                            break;
                        case WaypointTaskTransportAction.SetWaypointTo:
                            // Make sure the waypoint has at least whatever we specified
                            var currentAmount = dest.Cargo[transportTask.cargoType];

                            if (currentAmount >= transportTask.amount)
                            {
                                // no need to transfer any, move on
                                break;
                            }
                            else
                            {
                                // only transfer the min of what we have, vs what we need, vs the capacity
                                transferAmount = Math.Min(Math.Min(availableToUnload, transportTask.amount - currentAmount), dest.AvailableCapacity);
                                if (transferAmount < transportTask.amount)
                                {
                                    wp.WaitAtWaypoint = true;
                                }
                            }
                            break;
                        case WaypointTaskTransportAction.None:
                            break;
                        default:
                            log.Error($"{Game.Year}: {source.PlayerNum} {source.Name} Trying to process an unsupported unload task action: {transportTask.action}");
                            cargoToUnload = Cargo.Empty;
                            break;
                    }

                    if (transferAmount > 0)
                    {
                        // we are unloading so add cargo to the destination
                        Message.FleetTransportedCargo(task.Player, source, transportTask.cargoType, dest, transferAmount);
                        Transfer(source, dest, transportTask.cargoType, transferAmount);
                    }
                }
            }

            // we can't say this waypoint is processed, because it might have load tasks
            return false;
        }
    }
}