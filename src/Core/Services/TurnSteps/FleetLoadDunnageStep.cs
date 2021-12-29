using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetLoadDunnage0Step : FleetLoadDunnageStep
    {
        public FleetLoadDunnage0Step(
                IProvider<Game> gameProvider,
                IRulesProvider rulesProvider,
                CargoTransferer cargoTransferer,
                InvasionProcessor invasionProcessor)
            : base(gameProvider, rulesProvider, cargoTransferer, invasionProcessor, TurnGenerationState.FleetLoadDunnage0Step) { }
    }

    public class FleetLoadDunnage1Step : FleetLoadDunnageStep
    {
        public FleetLoadDunnage1Step(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            CargoTransferer cargoTransferer,
            InvasionProcessor invasionProcessor
        ) : base(gameProvider, rulesProvider, cargoTransferer, invasionProcessor, TurnGenerationState.FleetLoadDunnage1Step) { }
    }

    /// <summary>
    /// (minerals and colonists only) This command waits until all other loads and unloads are 
    /// complete, then loads as many colonists or amount of a mineral as will fit in the remaining 
    /// space. For example, setting Load All Germanium, Load Dunnage Ironium, will load all the 
    /// Germanium that is available, then as much Ironium as possible. If more than one dunnage cargo 
    /// is specified, they are loaded in the order of Ironium, Boranium, Germanium, and Colonists.
    /// </summary>
    public abstract class FleetLoadDunnageStep : AbstractFleetTransportStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetLoadDunnageStep));

        public FleetLoadDunnageStep(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            CargoTransferer cargoTransferer,
            InvasionProcessor invasionProcessor,
            TurnGenerationState state)
            : base(gameProvider, rulesProvider, cargoTransferer, invasionProcessor, state) { }

        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (source, wp, player) = task;

            if (wp.Target is ICargoHolder target)
            {
                foreach (var transportTask in task.TransportTasks.Where(transporttask => transporttask.cargoType != CargoType.Fuel))
                {
                    switch (transportTask.action)
                    {
                        case WaypointTaskTransportAction.LoadDunnage:
                            var capacity = source.Spec.CargoCapacity - source.Cargo.Total;
                            int availableToLoad = transportTask.cargoType == CargoType.Fuel ? target.Fuel : target.Cargo[transportTask.cargoType];

                            // load all available, based on our constraints
                            int transferAmount = Math.Min(availableToLoad, capacity);

                            if (transferAmount != 0)
                            {
                                // we are loading, so take cargo away from the source and add it to us
                                Message.FleetTransportedCargo(task.Player, source, transportTask.cargoType, target, -transferAmount);
                                Transfer(source, target, transportTask.cargoType, -transferAmount);
                            }
                            break;
                    }
                }
            }

            // Dunnage is the last step, so consider this waypoint processed.
            return true;
        }

    }
}