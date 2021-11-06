using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy scout ships
    /// </summary>
    public class PopulationRebalancerTurnProcessor : TurnProcessor
    {
        static CSLog log = LogProvider.GetLogger(typeof(PopulationRebalancerTurnProcessor));

        PlanetService planetService = new();
        FleetService fleetService = new();

        // the required population density required of a planet in order to suck people off of it
        // setting this to .25 because we don't want to suck people off a planet until it's reached the
        // max of its growth rate (over 1/4 crowded)
        private const float PopulationDensityRequired = .25f;

        public PopulationRebalancerTurnProcessor() : base("Population Rebalancer") { }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            // find the first colony ship design
            ShipDesign design = player.GetLatestDesign(ShipDesignPurpose.Freighter);

            var lowPopPlanets = player.Planets
            .Where(planet => planetService.GetPopulationDensity(planet, player, gameInfo.Rules) < PopulationDensityRequired)
            .OrderBy(planet => planet.Population)
            .ToList();
            var buildablePlanets = player.Planets
                .Where(planet => planetService.CanBuild(planet, player, design.Aggregate.Mass) && planetService.GetPopulationDensity(planet, player, gameInfo.Rules) >= PopulationDensityRequired)
                .ToList();
            var fleets = player.Fleets.Where(fleet => fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.Freighter));

            foreach (var fleet in fleets)
            {
                if (fleet.Waypoints.Count > 1 && fleet.Waypoints[1].Target is Planet planet)
                {
                    // don't send a colony ship to this planet
                    lowPopPlanets.Remove(planet);
                }
            }

            // go through each unassigned colonizer fleet and find it a new planet to colonize
            foreach (var fleet in fleets.Where(
                f => f.Waypoints.Count == 1 &&
                f.Orbiting?.PlayerNum == player.Num &&
                planetService.GetPopulationDensity(f.Orbiting, player, gameInfo.Rules, f.Orbiting.Population - f.AvailableCapacity) >= PopulationDensityRequired)
            )
            {
                if (lowPopPlanets.Count > 0)
                {
                    Planet lowPopPlanet = lowPopPlanets[0];
                    var sourcePlanet = fleet.Orbiting;
                    Cargo colonists = new Cargo(colonists: fleet.AvailableCapacity);

                    if (sourcePlanet.AttemptTransfer(-colonists) && fleet.AttemptTransfer(colonists))
                    {
                        CargoTransferUtils.CreateCargoTransferOrder(player, colonists, fleet, sourcePlanet);

                        // drop off colonists
                        var wp1 = Waypoint.TargetWaypoint(
                            lowPopPlanet,
                            fleetService.GetDefaultWarpFactor(fleet, player),
                            WaypointTask.Transport,
                            new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll)));
                        fleet.Waypoints.Add(wp1);

                        // return home
                        fleet.Waypoints.Add(Waypoint.TargetWaypoint(fleet.Orbiting, fleetService.GetDefaultWarpFactor(fleet, player)));

                        // remove this planet from our list
                        lowPopPlanets.Remove(lowPopPlanet);
                        log.Info($"{gameInfo.Year}: {player} {fleet.Name} assigned to transport {fleet.Cargo.Colonists * 100} colonists from {fleet.Orbiting.Name} to {lowPopPlanet.Name}");
                    }
                    else
                    {
                        log.Error($"{gameInfo.Year}: {player} Failed to transfer {fleet.AvailableCapacity}kT colonists from {sourcePlanet.Name} to {fleet.Name} for pop transfer.");
                    }
                }
            }

            BuildFleets(gameInfo, buildablePlanets, lowPopPlanets.Count, design);

        }

        /// <summary>
        /// Build any new colonizer fleets
        /// </summary>
        /// <param name="buildablePlanets"></param>
        /// <param name="numShipsNeeded"></param>
        /// <param name="design"></param>
        void BuildFleets(PublicGameInfo gameInfo, List<Planet> buildablePlanets, int numShipsNeeded, ShipDesign design)
        {
            if (design != null)
            {
                int queuedToBeBuilt = 0;

                List<Planet> planetsToBuildOn = new List<Planet>();
                foreach (Planet planet in buildablePlanets)
                {
                    bool isBuilding = false;
                    foreach (ProductionQueueItem item in planet.ProductionQueue?.Items)
                    {
                        if (item.Design != null && item.Design.Purpose == ShipDesignPurpose.Freighter)
                        {
                            isBuilding = true;
                            queuedToBeBuilt++;
                            log.Debug($"{gameInfo.Year}: {planet.Name} is already building a {item.Design.Purpose}");
                        }
                    }

                    // if this planet isn't already building a colony ship, build one
                    if (!isBuilding)
                    {
                        planetsToBuildOn.Add(planet);
                    }
                }

                foreach (Planet planet in planetsToBuildOn)
                {
                    // if this planet isn't building a colony ship already, add
                    // one to the queue
                    if (queuedToBeBuilt < numShipsNeeded)
                    {
                        planet.ProductionQueue.AddAfter(new ProductionQueueItem(QueueItemType.ShipToken, 1, design), QueueItemType.AutoFactories);
                        log.Debug($"{gameInfo.Year}: {planet.Name} building {design.Name} for pop transfer");
                        queuedToBeBuilt++;
                    }
                }

            }

        }
    }
}