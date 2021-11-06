using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy bomber fleets
    /// </summary>
    public class BomberTurnProcessor : TurnProcessor
    {
        static CSLog log = LogProvider.GetLogger(typeof(BomberTurnProcessor));

        PlanetService planetService = new();
        FleetService fleetService = new();

        public BomberTurnProcessor() : base("Bomber") { }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            // find the first colony ship design
            // TODO: pick the best one
            ShipDesign bomber = player.GetLatestDesign(ShipDesignPurpose.Bomber);

            if (bomber == null)
            {
                // we have no bomber design, can't bomb planets
                // NOTE: The ShipDesignTurnProcessor should handle this
                return;
            }

            var bombablePlanets = player.AllPlanets.Where(planet =>
                planet.Explored &&
                planet.Owned &&
                !planet.OwnedBy(player)
            ).ToList();
            var buildablePlanets = player.Planets.Where(planet => planetService.CanBuild(planet, player, bomber.Aggregate.Mass)).ToList();
            var bomberFleets = player.Fleets.Where(fleet => fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.Bomber));

            foreach (var fleet in bomberFleets)
            {
                if (fleet.Waypoints.Count > 1 && fleet.Waypoints[1].Target is Planet planet)
                {
                    // don't send a colony ship to this planet
                    bombablePlanets.Remove(planet);
                }
            }

            // go through each unassigned colonizer fleet and find it a new planet to colonize
            foreach (var fleet in bomberFleets.Where(f => f.Waypoints.Count == 1))
            {

                Planet planetToBomb = ClosestPlanet(fleet, bombablePlanets);
                if (planetToBomb != null && fleet.Waypoints[0].Target != planetToBomb)
                {
                    fleet.Waypoints.Add(Waypoint.TargetWaypoint(planetToBomb, fleetService.GetDefaultWarpFactor(fleet, player)));
                    fleet.Waypoints[1].WarpFactor = fleetService.GetBestWarpFactor(fleet, player, fleet.Waypoints[0], fleet.Waypoints[1]);

                    // remove this planet from our list
                    bombablePlanets.Remove(planetToBomb);
                }
            }

            BuildFleets(buildablePlanets, bombablePlanets.Count, bomber);
        }

        /// <summary>
        /// Build any new colonizer fleets
        /// </summary>
        /// <param name="buildablePlanets"></param>
        /// <param name="numShipsNeeded"></param>
        /// <param name="design"></param>
        void BuildFleets(List<Planet> buildablePlanets, int numShipsNeeded, ShipDesign design)
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
                        if (item.Design != null && item.Design.Purpose == ShipDesignPurpose.Bomber)
                        {
                            isBuilding = true;
                            queuedToBeBuilt++;
                            log.Debug($"planet {planet.Name} is already building {design.Name}");
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
                        planet.ProductionQueue?.Items.Add(new ProductionQueueItem(QueueItemType.ShipToken, 1, design));
                        log.Debug($"Added {design.Name} to planet queue: {planet.Name}");
                        queuedToBeBuilt++;
                    }
                }

            }

        }
    }
}