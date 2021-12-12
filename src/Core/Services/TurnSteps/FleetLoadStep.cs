using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetLoad0Step : FleetLoadStep
    {
        public FleetLoad0Step(
                IProvider<Game> gameProvider,
                IRulesProvider rulesProvider,
                InvasionProcessor invasionProcessor,
                FleetService fleetService
            ) : base(gameProvider, rulesProvider, invasionProcessor, fleetService, TurnGenerationState.FleetLoad0Step) { }
    }

    public class FleetLoad1Step : FleetLoadStep
    {
        public FleetLoad1Step(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            InvasionProcessor invasionProcessor,
            FleetService fleetService
        ) : base(gameProvider, rulesProvider, invasionProcessor, fleetService, TurnGenerationState.FleetLoad1Step) { }
    }

    /// <summary>
    /// Process all load cargo tasks
    /// </summary>
    public abstract class FleetLoadStep : AbstractFleetTransportStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetLoadStep));

        private readonly FleetService fleetService;

        public FleetLoadStep(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            InvasionProcessor invasionProcessor,
            FleetService fleetService,
            TurnGenerationState state)
            : base(gameProvider, rulesProvider, invasionProcessor, state)
        {
            this.fleetService = fleetService;
        }

        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (source, wp, player) = task;

            if (wp.Target is ICargoHolder dest)
            {
                foreach (var transportTask in task.TransportTasks)
                {
                    var capacity = source.Spec.CargoCapacity - source.Cargo.Total;
                    int transferAmount = 0;
                    int availableToLoad = transportTask.cargoType == CargoType.Fuel ? dest.Fuel : dest.Cargo[transportTask.cargoType];

                    // the current amount of this cargoType we have in our fleet
                    var currentAmount = transportTask.cargoType == CargoType.Fuel ? source.Fuel : source.Cargo[transportTask.cargoType];

                    switch (transportTask.action)
                    {
                        case WaypointTaskTransportAction.LoadOptimal:
                            // fuel only
                            // we set our fuel to whatever it takes to finish our waypoints and transfer the rest to the ICargoHolder target.
                            // If the target is a planet or starbase (and has infinite fuel capacity), we skip this and don't give them our fuel
                            if (transportTask.cargoType == CargoType.Fuel && dest.FuelCapacity != MapObject.Infinite)
                            {
                                int fuelRequiredForNextWaypoint = 0;
                                var wp0 = source.Waypoints[0];
                                foreach (var wpNext in source.Waypoints.Skip(1))
                                {
                                    fuelRequiredForNextWaypoint += fleetService.GetFuelCost(source, player, wpNext.WarpFactor, wp0.Position.DistanceTo(wpNext.Position));
                                }

                                int leftoverFuel = source.Fuel - fuelRequiredForNextWaypoint;
                                int fuelCapacityAvailable = dest.FuelCapacity - dest.Fuel;
                                if (leftoverFuel > 0 && fuelCapacityAvailable > 0)
                                {
                                    // transfer the lowest of how much fuel capacity they have available or how much we can give
                                    // this is a bit weird because we are doing a "Load", but it's actually an unload of fuel
                                    // from us to a dest fleet, so make the transferAmount negative.
                                    transferAmount = Math.Max(-leftoverFuel, -(dest.FuelCapacity - dest.Fuel));
                                }
                            }
                            break;
                        case WaypointTaskTransportAction.LoadAll:
                            // load all available, based on our constraints
                            transferAmount = Math.Min(availableToLoad, capacity);
                            break;
                        case WaypointTaskTransportAction.LoadAmount:
                            transferAmount = Math.Min(Math.Min(availableToLoad, transportTask.amount), capacity);
                            break;
                        case WaypointTaskTransportAction.WaitForPercent:
                        case WaypointTaskTransportAction.FillPercent:
                            // we want a percent of our hold to be filled with some amount, figure out how
                            // much that is in kT, i.e. 50% of 100kT would be 50kT of this mineral
                            var taskAmountkT = (int)((float)transportTask.amount / 100f * source.Spec.CargoCapacity);

                            if (currentAmount >= taskAmountkT)
                            {
                                // no need to transfer any, move on
                                break;
                            }
                            else
                            {
                                // only transfer the min of what we have, vs what we need, vs the capacity
                                transferAmount = Math.Min(Math.Min(availableToLoad, taskAmountkT - currentAmount), capacity);
                                if (transferAmount < taskAmountkT && transportTask.action == WaypointTaskTransportAction.WaitForPercent)
                                {
                                    wp.WaitAtWaypoint = true;
                                }
                            }
                            break;
                        case WaypointTaskTransportAction.SetAmountTo:
                            if (currentAmount >= transportTask.amount)
                            {
                                // no need to transfer any, move on
                                break;
                            }
                            else
                            {
                                // only transfer the min of what we have, vs what we need, vs the capacity
                                transferAmount = Math.Min(Math.Min(availableToLoad, transportTask.amount - currentAmount), capacity);
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
                            break;
                    }

                    if (transferAmount != 0)
                    {
                        // we are loading, so take cargo away from the dest and add it to us
                        Message.FleetTransportedCargo(task.Player, source, transportTask.cargoType, dest, -transferAmount);
                        Transfer(source, dest, transportTask.cargoType, -transferAmount);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// (minerals and colonists only) This command waits until all other loads and unloads are 
        /// complete, then loads as many colonists or amount of a mineral as will fit in the remaining 
        /// space. For example, setting Load All Germanium, Load Dunnage Ironium, will load all the 
        /// Germanium that is available, then as much Ironium as possible. If more than one dunnage cargo 
        /// is specified, they are loaded in the order of Ironium, Boranium, Germanium, and Colonists.
        /// </summary>
        /// <param name="fleetWaypoint"></param>
        void ProcessLoadDunnageTask(FleetWaypointProcessTask fleetWaypoint)
        {
            var source = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;
            var player = fleetWaypoint.Player;

            if (wp.Target is ICargoHolder target)
            {
                foreach (var task in fleetWaypoint.TransportTasks.Where(task => task.cargoType != CargoType.Fuel))
                {
                    var capacity = source.Spec.CargoCapacity - source.Cargo.Total;
                    int availableToLoad = task.cargoType == CargoType.Fuel ? target.Fuel : target.Cargo[task.cargoType];

                    // load all available, based on our constraints
                    int transferAmount = Math.Min(availableToLoad, capacity);

                    if (transferAmount != 0)
                    {
                        // we are loading, so take cargo away from the source and add it to us
                        Message.FleetTransportedCargo(fleetWaypoint.Player, source, task.cargoType, target, -transferAmount);
                        Transfer(source, target, task.cargoType, -transferAmount);
                    }
                }
            }
        }



    }
}