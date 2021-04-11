using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Process Waypoint 0 actions
    /// Note: this will be called twice, once at the beginning of a turn, and once after fleets move
    ///
    /// From TurnGenerator.cs: 
    ///     Scrapping fleets (w/possible tech gain) 
    ///     Waypoint 0 unload tasks 
    ///     Waypoint 0 Colonization/Ground Combat resolution (w/possible tech gain) 
    ///     Waypoint 0 load tasks 
    ///     Other Waypoint 0 tasks * 
    /// </summary>
    public class FleetWaypointStep : Step
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetWaypointStep));
        public const string ProcessedWaypointsContextKey = "ProcessedWaypoints";

        HashSet<Waypoint> processedWaypoints = new HashSet<Waypoint>();
        List<Fleet> scrappedFleets = new List<Fleet>();

        List<FleetWaypoint> ScrapFleetTasks = new List<FleetWaypoint>();
        List<FleetWaypoint> UnloadTasks = new List<FleetWaypoint>();
        List<FleetWaypoint> ColonizeTasks = new List<FleetWaypoint>();
        List<FleetWaypoint> LoadTasks = new List<FleetWaypoint>();
        List<FleetWaypoint> LoadDunnageTasks = new List<FleetWaypoint>();
        List<FleetWaypoint> OtherTasks = new List<FleetWaypoint>();
        List<PlanetInvasion> Invasions = new List<PlanetInvasion>();

        InvasionProcessor invasionProcessor;

        public FleetWaypointStep(Game game, TurnGeneratorState state) : base(game, state) { }

        /// <summary>
        /// Override PreProcess() to get processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            invasionProcessor = new InvasionProcessor();
            processedWaypoints.Clear();
            scrappedFleets.Clear();

            if (Context.Context.TryGetValue(ProcessedWaypointsContextKey, out var waypoints)
            && waypoints is HashSet<Waypoint> processedWaypointsFromContext)
            {
                // add any processed waypoints from the context
                processedWaypoints.UnionWith(processedWaypointsFromContext);
            }
        }

        public override void Process()
        {

            // Separate our waypoint tasks into groups
            Game.Fleets.ForEach(fleet => BuildWaypointTasks(fleet));

            // scrap
            ScrapFleetTasks.ForEach(task => ProcessScrapFleetTask(task));
            ColonizeTasks.ForEach(task => ProcessColonizeTask(task));
            scrappedFleets.ForEach(fleet => EventManager.PublishFleetDeletedEvent(fleet));

            // 
            UnloadTasks.ForEach(task => ProcessUnloadTask(task));
            Invasions.ForEach(task => ProcessInvasionTask(task));
            LoadTasks.ForEach(task => ProcessLoadTask(task));
            LoadDunnageTasks.ForEach(task => ProcessLoadDunnageTask(task));
            OtherTasks.ForEach(task => ProcessOtherTask(task));

            // notify the player about idle fleets after
            // we have scrapped, colonized, etc
            Game.Fleets.ForEach(fleet => NotifyIdleFleet(fleet));
        }

        /// <summary>
        /// Override PostProcess() to set processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PostProcess()
        {
            base.PostProcess();
            Context.Context[ProcessedWaypointsContextKey] = processedWaypoints;
        }

        /// <summary>
        /// For each waypoint, split it into separate tasks because we process all scrap, 
        /// then unload, then colonize, then load, etc tasks in groups
        /// </summary>
        /// <param name="wp"></param>
        public void BuildWaypointTasks(Fleet fleet)
        {
            if (fleet.Waypoints.Count > 0)
            {
                var wp = fleet.Waypoints[0];
                if (processedWaypoints.Contains(wp))
                {
                    // we already processed this during wp0, don't process it again
                    return;
                }

                log.Debug($"{Game.Year}: {fleet.Player} Building waypoint task for {fleet.Name} at {wp.TargetName} -> {wp.Task}");

                switch (wp.Task)
                {
                    case WaypointTask.Colonize:
                        ColonizeTasks.Add(new FleetWaypoint(fleet, wp));
                        break;
                    case WaypointTask.LayMineField:
                    case WaypointTask.MergeWithFleet:
                    case WaypointTask.Patrol:
                    case WaypointTask.RemoteMining:
                    case WaypointTask.Route:
                    case WaypointTask.TransferFleet:
                        OtherTasks.Add(new FleetWaypoint(fleet, wp));
                        break;
                    case WaypointTask.ScrapFleet:
                        ScrapFleetTasks.Add(new FleetWaypoint(fleet, wp));
                        break;
                    case WaypointTask.Transport:
                        var unload = new FleetWaypoint(fleet, wp);
                        var load = new FleetWaypoint(fleet, wp);
                        var loadDunnage = new FleetWaypoint(fleet, wp);
                        foreach (CargoType type in Enum.GetValues(typeof(CargoType)))
                        {
                            var task = wp.TransportTasks[type];
                            switch (task.action)
                            {
                                case WaypointTaskTransportAction.LoadAll:
                                case WaypointTaskTransportAction.LoadAmount:
                                case WaypointTaskTransportAction.FillPercent:
                                case WaypointTaskTransportAction.WaitForPercent:
                                    load.AddTask(task, type);
                                    break;
                                case WaypointTaskTransportAction.UnloadAll:
                                case WaypointTaskTransportAction.UnloadAmount:
                                    unload.AddTask(task, type);
                                    break;
                                case WaypointTaskTransportAction.LoadDunnage:
                                    loadDunnage.AddTask(task, type);
                                    break;
                                case WaypointTaskTransportAction.SetAmountTo:
                                case WaypointTaskTransportAction.SetWaypointTo:
                                    // these could be load or unload, we won't know until we process
                                    // them, so assume they could be either
                                    unload.AddTask(task, type);
                                    load.AddTask(task, type);
                                    break;
                            }
                        }
                        // split these orders into load/unload/dunnage
                        if (unload.Tasks.Count > 0)
                        {
                            UnloadTasks.Add(unload);
                        }
                        if (load.Tasks.Count > 0)
                        {
                            LoadTasks.Add(load);
                        }
                        if (loadDunnage.Tasks.Count > 0)
                        {
                            LoadDunnageTasks.Add(load);
                        }
                        break;

                }
            }
            else
            {
                log.Error($"{Game.Year}: {fleet.Player} {fleet.Name} has 0 waypoints.");
            }
        }

        /// <summary>
        /// Process an unload task. This will unload each cargo type to the target, assuming it can hold it
        /// </summary>
        /// <param name="task"></param>
        void ProcessUnloadTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;
            if (wp.Target is ICargoHolder cargoDestination)
            {
                Cargo cargoToUnload;
                foreach (var task in fleetWaypoint.Tasks)
                {
                    int transferAmount = 0;
                    switch (task.action)
                    {
                        case WaypointTaskTransportAction.UnloadAll:
                            // load all available, based on our constraints
                            var availableToUnload = fleet.Cargo[task.cargoType];
                            transferAmount = Math.Min(availableToUnload, cargoDestination.AvailableCapacity);
                            break;
                        case WaypointTaskTransportAction.UnloadAmount:
                            break;
                        case WaypointTaskTransportAction.SetAmountTo:
                        case WaypointTaskTransportAction.SetWaypointTo:
                            break;
                        default:
                            log.Error($"{Game.Year}: {fleet.Player} {fleet.Name} Trying to process an unsupported unload task action: {task.action}");
                            cargoToUnload = Cargo.Empty;
                            break;
                    }

                    if (transferAmount > 0)
                    {
                        // we are unloading so add cargo to the destination
                        Transfer(fleet, cargoDestination, task.cargoType, transferAmount);
                    }
                }
            }
        }

        /// <summary>
        /// Process an unload task. This will unload each cargo type to the target, assuming it can hold it
        /// </summary>
        /// <param name="task"></param>
        void ProcessLoadTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;
            if (wp.Target is ICargoHolder cargoSource)
            {
                var capacity = fleet.Aggregate.CargoCapacity - fleet.Cargo.Total;
                Cargo cargoToUnload;
                foreach (var task in fleetWaypoint.Tasks)
                {
                    int transferAmount = 0;
                    switch (task.action)
                    {
                        case WaypointTaskTransportAction.LoadAll:
                            // load all available, based on our constraints
                            var availableToLoad = cargoSource.Cargo[task.cargoType];
                            transferAmount = Math.Min(availableToLoad, capacity);
                            break;
                        case WaypointTaskTransportAction.LoadAmount:
                            break;
                        case WaypointTaskTransportAction.LoadDunnage:
                            break;
                        case WaypointTaskTransportAction.SetAmountTo:
                        case WaypointTaskTransportAction.SetWaypointTo:
                            break;
                        default:
                            log.Error($"{Game.Year}: {fleet.Player} {fleet.Name} Trying to process an unsupported unload task action: {task.action}");
                            cargoToUnload = Cargo.Empty;
                            break;
                    }

                    if (transferAmount > 0)
                    {
                        // we are loading, so take cargo away from the source and add it to us
                        Transfer(fleet, cargoSource, task.cargoType, -transferAmount);
                    }
                }
            }
        }


        /// <summary>
        /// Transfer minerals or fuel to/from the fleet to a cargoDestination (planet, fleet, deep space, etc)
        /// This will create PlanetInvasions if we beam our colonists to an enemy planet
        /// </summary>
        /// <param name="fleet">The fleet transferring cargo</param>
        /// <param name="cargoHolderTarget">The source or destination giving or receiving the cargo</param>
        /// <param name="cargoType">The type of cargo being transferred</param>
        /// <param name="transferAmount">The amount of cargo being transferred</param>
        void Transfer(Fleet fleet, ICargoHolder cargoHolderTarget, CargoType cargoType, int transferAmount)
        {
            if (cargoType == CargoType.Fuel)
            {
                // fuel transfer
                fleet.AttemptTransfer(Cargo.Empty, fuelTransfer: -transferAmount);
                cargoHolderTarget.AttemptTransfer(Cargo.Empty, fuel: transferAmount);
            }
            else if (cargoType == CargoType.Colonists)
            {
                // invasion?
                if (cargoHolderTarget is Planet planet && planet.Player != fleet.Player)
                {
                    if (transferAmount > 0)
                    {
                        Invasions.Add(new PlanetInvasion()
                        {
                            Planet = planet,
                            Fleet = fleet,
                            ColonistsToDrop = transferAmount
                        });
                        // remove colonists from our cargo
                        fleet.Cargo = fleet.Cargo - Cargo.OfAmount(cargoType, transferAmount);
                    }
                    else
                    {
                        // can't beam enemy colonists onto your ship...
                        // TODO: send a message
                        log.Warn($"{Game.Year}: {fleet.Player} {fleet.Name} tried to beam colonists up from: {cargoHolderTarget}");
                    }
                }
                else if (cargoHolderTarget is Fleet otherFleet && otherFleet.Player != fleet.Player)
                {
                    // ignore this, but send a message
                    // TODO: send a message
                    log.Warn($"{Game.Year}: {fleet.Player} {fleet.Name} tried to transfer colonists to/from a fleet they don't own: {otherFleet}");

                }
                else
                {
                    // this is just a regular transfer
                    fleet.AttemptTransfer(Cargo.OfAmount(cargoType, -transferAmount));
                    cargoHolderTarget.AttemptTransfer(Cargo.OfAmount(cargoType, transferAmount), 0);
                    log.Debug($"{Game.Year}: {fleet.Player} {fleet.Name} ");
                }
            }
            else
            {
                fleet.AttemptTransfer(Cargo.OfAmount(cargoType, -transferAmount), 0);
                cargoHolderTarget.AttemptTransfer(Cargo.OfAmount(cargoType, transferAmount), 0);
            }
        }

        /// <summary>
        /// After we build a list of planet invasions, run them
        /// </summary>
        void ProcessInvasionTask(PlanetInvasion task)
        {
            invasionProcessor.InvadePlanet(task.Planet, task.Fleet.Player, task.Fleet, task.ColonistsToDrop);
        }


        /// <summary>
        /// Scrap this fleet, adding resources to the waypoint
        /// from the stars wiki:
        /// After battle, 1/3 of the mineral cost of the destroyed ships is left as salvage. If the battle took place in orbit, these minerals are deposited on the planet below.
        /// In deep space, each type of mineral decays 10%, or 10kT per year, whichever is higher. Salvage deposited on planets does not decay.
        /// Scrapping: (from help file)
        /// 
        /// A ship scrapped at a starbase deposits 80% of the original minerals on the planet, or 90% of the minerals and 70% of the resources if the LRT 'Ultimate Recycling' is selected.
        /// A ship scrapped at a planet with no starbase leaves 33% of the original minerals on the planet, or 45% of the minerals if the LRT Ultimate Recycling is selected.
        /// Wih UR the resources recovered is:
        /// (resources the ship costs * resources on the planet)/(resources the ship cost + resources on the planet)
        /// The maximum recoverable resources occurs when the cost of the scrapped ship equals the resources produced at the planet where it is scrapped.
        /// 
        /// A ship scrapped in space leaves no minerals behind.
        /// When a ship design is deleted, all such ships vanish leaving nothing behind. (moral: scrap before you delete!)
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>    
        void ProcessScrapFleetTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;

            // create a new cargo instance out of our fleet cost
            Cargo cargo = fleet.Aggregate.Cost;

            if (fleet.Player.Race.HasLRT(LRT.UR))
            {
                // we only recover 40% of our minerals on scrapping
                cargo *= .45f;

                // TODO: handle resource gain for an occupied planet
            }
            else
            {
                // we only recover 1/3rd of our minerals on scrapping
                cargo /= 3;
            }

            // add in any cargo the fleet was holding
            cargo += fleet.Cargo;

            if (wp.Target is Planet planet)
            {
                planet.Cargo += cargo;
                fleet.Scrapped = true;
                scrappedFleets.Add(fleet);
            }
            else
            {
                // TODO: put some scrap in space...
            }
        }

        /// <summary>
        /// Take this fleet and have it colonize a planet
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>
        void ProcessColonizeTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;

            if (wp.Target is Planet planet)
            {
                if (planet.Player != null)
                {
                    Message.ColonizeOwnedPlanet(fleet.Player, fleet);
                }
                else if (!fleet.Aggregate.Colonizer)
                {
                    Message.ColonizeWithNoModule(fleet.Player, fleet);
                }
                else if (fleet.Cargo.Colonists <= 0)
                {
                    Message.ColonizeWithNoColonists(fleet.Player, fleet);
                }
                else
                {
                    // we own this planet now, yay!
                    planet.Player = fleet.Player;
                    planet.ProductionQueue = new ProductionQueue();
                    planet.Population = fleet.Cargo.Colonists * 100;
                    fleet.Cargo = fleet.Cargo.WithColonists(0);
                    Message.PlanetColonized(fleet.Player, planet);
                    ProcessScrapFleetTask(fleetWaypoint);
                }
            }
            else
            {
                Message.ColonizeNonPlanet(fleet.Player, fleet);
            }
        }

        /// <summary>
        /// (minerals and colonists only) This command waits until all other loads and unloads are 
        /// complete, then loads as many colonists or amount of a mineral as will fit in the remaining 
        /// space. For example, setting Load All Germanium, Load Dunnage Ironium, will load all the 
        /// Germanium that is available, then as much Ironium as possible. If more than one dunnage cargo 
        /// is specified, they are loaded in the order of Ironium, Boranium, Germanium, and Colonists.
        /// </summary>
        /// <param name="fleetWaypoint"></param>
        void ProcessLoadDunnageTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;

            // TODO: figure out dunnage...
        }

        /// <summary>
        /// Process tasks actions that happen after scrap/load/unload/colonize/invasion
        /// </summary>
        /// <param name="fleetWaypoint"></param>
        void ProcessOtherTask(FleetWaypoint fleetWaypoint)
        {
            var fleet = fleetWaypoint.Fleet;
            var wp = fleetWaypoint.Waypoint;

            switch (wp.Task)
            {
                case WaypointTask.LayMineField:
                    break;
                case WaypointTask.MergeWithFleet:
                    break;
                case WaypointTask.Patrol:
                    break;
                case WaypointTask.RemoteMining:
                    break;
                case WaypointTask.Route:
                    break;
                case WaypointTask.TransferFleet:
                    break;
            }
        }

        /// <summary>
        /// Notify the Player if this fleet has completed it's assigned task
        /// </summary>
        /// <param name="fleet"></param>
        void NotifyIdleFleet(Fleet fleet)
        {
            if (fleet.Waypoints.Count == 1)
            {
                // some orders are continuous, otherwise this fleet is done
                var wp = fleet.Waypoints[0];
                if (wp.Task != WaypointTask.LayMineField &&
                    wp.Task != WaypointTask.RemoteMining &&
                    wp.Task != WaypointTask.Patrol)
                    Message.FleetCompletedAssignedOrders(fleet.Player, fleet);
            }
        }

    }
}