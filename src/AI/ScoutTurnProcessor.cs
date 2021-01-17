using System;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy scout ships
    /// </summary>
    public class ScoutTurnProcessor : TurnProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ScoutTurnProcessor));

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(int year, UniverseSettings settings, Player player)
        {

            // find the first scout ship design
            // TODO: pick the best one
            ShipDesign scoutShip = player.Designs.Find(design => design.Hull.Name == "Scout");

            // find all the planets we don't know about yet
            List<Planet> unknownPlanets = player.Planets.Where(planet => !planet.Explored).ToList();
            List<Planet> buildablePlanets = player.Planets.Where(planet => planet.CanBuild(player, scoutShip.Aggregate.Mass)).ToList();

            // get all the fleets that can scan and don't have waypoints yet
            List<Fleet> scannerFleets = new List<Fleet>();

            foreach (Fleet fleet in player.Fleets.Where(fleet => fleet.OwnedBy(player) && fleet.Aggregate.Scanner))
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
                    fleet.Waypoints.Add(new Waypoint(planetToScout, fleet.GetDefaultWarpFactor(), WaypointTask.None));

                    // remove this planet from our list
                    unknownPlanets.Remove(planetToScout);
                }
            }

            BuildFleets(buildablePlanets, unknownPlanets.Count, scoutShip);

        }

        /// <summary>
        /// Build any new colonizer fleets
        /// </summary>
        /// <param name="buildablePlanets"></param>
        /// <param name="numShipsNeeded"></param>
        /// <param name="scoutShip"></param>
        void BuildFleets(List<Planet> buildablePlanets, int numShipsNeeded, ShipDesign scoutShip)
        {
            if (scoutShip != null)
            {

                int queuedToBeBuilt = 0;

                List<Planet> planetsToBuildOn = new List<Planet>();
                foreach (Planet planet in buildablePlanets)
                {
                    bool isBuilding = false;
                    foreach (ProductionQueueItem item in planet.ProductionQueue.Items)
                    {
                        if (item.Design != null && item.Design.Aggregate.Scanner)
                        {
                            isBuilding = true;
                            queuedToBeBuilt++;
                            log.Debug($"planet {planet.Name} is already building a scout ship");
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
                        planet.ProductionQueue.Items.Add(new ProductionQueueItem(QueueItemType.ShipToken, 1, scoutShip));
                        log.Debug($"Added scout ship to planet queue: {planet.Name}");
                        queuedToBeBuilt++;
                    }
                }

            }

        }
    }
}