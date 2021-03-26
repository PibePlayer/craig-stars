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
    /// </summary>
    public class FleetWaypointStep : Step
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetWaypointStep));
        public const string ProcessedWaypointsContextKey = "ProcessedWaypoints";

        public FleetWaypointStep(Game game, TurnGeneratorState state) : base(game, state) { }

        HashSet<Waypoint> processedWaypoints = new HashSet<Waypoint>();
        List<Fleet> scrappedFleets = new List<Fleet>();

        /// <summary>
        /// Override PreProcess() to get processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

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
            Game.Fleets.ForEach(fleet => ProcessWaypoint(fleet));
            scrappedFleets.ForEach(fleet => EventManager.PublishFleetDeletedEvent(fleet));
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
        /// Process the task at the current waypoint
        /// </summary>
        /// <param name="wp"></param>
        public void ProcessWaypoint(Fleet fleet)
        {
            if (fleet.Waypoints.Count > 0)
            {
                var wp = fleet.Waypoints[0];
                if (processedWaypoints.Contains(wp))
                {
                    // we already processed this during wp0, don't process it again
                    return;
                }
                log.Debug($"Processing waypoint for {fleet.Name} at {wp.TargetName} -> {wp.Task}");

                switch (wp.Task)
                {
                    case WaypointTask.Colonize:
                        Colonize(fleet, wp);
                        break;
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
                    case WaypointTask.ScrapFleet:
                        Scrap(fleet, wp);
                        break;
                    case WaypointTask.TransferFleet:
                        break;
                    case WaypointTask.Transport:
                        Transport(fleet, wp);
                        break;

                }
                processedWaypoints.Add(wp);

                // if we are done, 
                if (fleet.Waypoints.Count == 1 && !fleet.Scrapped)
                {
                    Message.FleetCompletedAssignedOrders(fleet.Player, fleet);
                }
            }

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
        void Scrap(Fleet fleet, Waypoint wp)
        {
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
        void Colonize(Fleet fleet, Waypoint wp)
        {
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
                    Scrap(fleet, wp);
                }
            }
            else
            {
                Message.ColonizeNonPlanet(fleet.Player, fleet);
            }
        }

        void Transport(Fleet fleet, Waypoint wp)
        {
            if (wp.Target is ICargoHolder cargoHolder)
            {
                // how much space do we have available
                var capacity = fleet.Aggregate.CargoCapacity - fleet.Cargo.Total;

                foreach (CargoType taskType in Enum.GetValues(typeof(CargoType)))
                {
                    var task = wp.TransportTasks[taskType];
                    int transferAmount = 0;
                    switch (task.action)
                    {
                        case WaypointTaskTransportAction.LoadAll:
                            // load all available, based on our constraints
                            var availableToLoad = cargoHolder.Cargo[taskType];
                            transferAmount = Math.Min(availableToLoad, capacity);
                            if (transferAmount > 0)
                            {
                                if (taskType == CargoType.Fuel)
                                {
                                    cargoHolder.AttemptTransfer(Cargo.Empty, -transferAmount);
                                    fleet.AttemptTransfer(Cargo.Empty, transferAmount);
                                }
                                else
                                {
                                    cargoHolder.AttemptTransfer(Cargo.OfAmount(taskType, -transferAmount), 0);
                                    fleet.AttemptTransfer(Cargo.OfAmount(taskType, transferAmount), 0);
                                }
                            }
                            break;
                        case WaypointTaskTransportAction.UnloadAll:
                            // load all available, based on our constraints
                            var availableToUnload = fleet.Cargo[taskType];
                            transferAmount = Math.Min(availableToUnload, cargoHolder.AvailableCapacity);
                            if (transferAmount > 0)
                            {
                                if (taskType == CargoType.Fuel)
                                {
                                    fleet.AttemptTransfer(Cargo.Empty, -transferAmount);
                                    cargoHolder.AttemptTransfer(Cargo.Empty, transferAmount);
                                }
                                else if (taskType == CargoType.Colonists)
                                {
                                    // invasion?
                                    if (wp.Target is Planet planet && planet.Player != fleet.Player)
                                    {
                                        PlanetInvader planetInvader = new PlanetInvader();
                                        planetInvader.InvadePlanet(planet, fleet.Player, fleet, transferAmount);
                                    }
                                }
                                else
                                {
                                    fleet.AttemptTransfer(Cargo.OfAmount(taskType, -transferAmount), 0);
                                    cargoHolder.AttemptTransfer(Cargo.OfAmount(taskType, transferAmount), 0);
                                }
                            }
                            break;
                    }

                }
            }
        }
    }
}