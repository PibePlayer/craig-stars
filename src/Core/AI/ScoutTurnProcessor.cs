using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy scout ships
    /// </summary>
    public class ScoutTurnProcessor : TurnProcessor
    {
        static CSLog log = LogProvider.GetLogger(typeof(ScoutTurnProcessor));

        private readonly PlanetService planetService;
        private readonly FleetService fleetService;

        public ScoutTurnProcessor(PlanetService planetService, FleetService fleetService) : base("Scouter")
        {
            this.planetService = planetService;
            this.fleetService = fleetService;
        }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {

            // find the first scout ship design
            // TODO: pick the best one
            ShipDesign scoutShip = player.GetLatestDesign(ShipDesignPurpose.Scout);

            log.Debug($"{gameInfo.Year}: {player} Found best scout design: {scoutShip.Name} v{scoutShip.Version}");

            // find all the planets we don't know about yet
            List<Planet> unknownPlanets = player.AllPlanets.Where(planet => !planet.Explored).ToList();
            List<Planet> buildablePlanets = player.Planets.Where(planet => planetService.CanBuild(planet, player, scoutShip.Aggregate.Mass)).ToList();

            // get all the fleets that can scan and don't have waypoints yet
            List<Fleet> scannerFleets = new List<Fleet>();

            foreach (Fleet fleet in player.Fleets.Where(fleet => fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.Scout)))
            {
                if (fleet.Waypoints.Count == 1)
                {
                    // add this fleet because it's not going anywhere yet
                    scannerFleets.Add(fleet);
                }
                else
                {
                    // check if this fleet is already headed to a planet.
                    if (fleet.Waypoints.Count > 1 && fleet.Waypoints[1].Target is Planet planet)
                    {
                        // remove this planet from our list of planets to scan
                        unknownPlanets.Remove(planet);
                    }
                }
            }

            // go through each scanner fleet and find it a new planet to scout
            foreach (Fleet fleet in scannerFleets)
            {
                Planet planetToScout = ClosestPlanet(fleet, unknownPlanets);
                if (planetToScout != null)
                {
                    // add this planet as a waypoint
                    fleet.Waypoints.Add(Waypoint.TargetWaypoint(planetToScout, fleetService.GetDefaultWarpFactor(fleet, player), WaypointTask.None));
                    fleet.Waypoints[1].WarpFactor = fleetService.GetBestWarpFactor(fleet, player, fleet.Waypoints[0], fleet.Waypoints[1]);

                    // remove this planet from our list
                    unknownPlanets.Remove(planetToScout);
                }
            }

            BuildFleets(gameInfo, player, buildablePlanets, unknownPlanets.Count, scoutShip);

        }

        /// <summary>
        /// Build any new colonizer fleets
        /// </summary>
        /// <param name="buildablePlanets"></param>
        /// <param name="numShipsNeeded"></param>
        /// <param name="scoutShip"></param>
        void BuildFleets(PublicGameInfo gameInfo, Player player, List<Planet> buildablePlanets, int numShipsNeeded, ShipDesign scoutShip)
        {
            if (scoutShip != null)
            {

                int queuedToBeBuilt = 0;

                List<Planet> planetsToBuildOn = new List<Planet>();
                foreach (Planet planet in buildablePlanets)
                {
                    bool isBuilding = false;
                    foreach (ProductionQueueItem item in planet.ProductionQueue?.Items)
                    {
                        if (item.Design != null && item.Design.Aggregate.Scanner)
                        {
                            isBuilding = true;
                            queuedToBeBuilt++;
                            log.Debug($"{gameInfo.Year}: {planet.PlayerNum} planet {planet.Name} is already building a scout ship");
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
                        // put this at the top of the queue
                        // above any auto items
                        planet.ProductionQueue?.Items.Insert(0, new ProductionQueueItem(QueueItemType.ShipToken, 1, scoutShip));
                        log.Debug($"{gameInfo.Year}: {planet.PlayerNum} Added scout ship to planet queue: {planet.Name}");
                        queuedToBeBuilt++;
                    }
                }

            }

        }
    }
}