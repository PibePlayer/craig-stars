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
    public class ColonyTurnProcessor : TurnProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ScoutTurnProcessor));

        // the required population density required of a planet in order to suck people off of it
        // setting this to .33 because we don't want to suck people off a planet until it's reached the
        // max of its growth rate (over 1/3rd crowded)
        private const float PopulationDensityRequired = .33f;

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(int year, UniverseSettings settings, Player player)
        {
            // find the first colony ship design
            // TODO: pick the best one
            ShipDesign colonyShip = player.Designs.Find(design => design.Hull.Name == "Colony Ship");

            var colonizablePlanets = player.Planets.Where(planet => planet.Explored && planet.Uninhabited && player.Race.GetPlanetHabitability(planet.Hab.Value) > 0).ToList();
            var buildablePlanets = player.Planets.Where(planet => planet.CanBuild(player, colonyShip.Aggregate.Mass)).ToList();
            var colonizerFleets = player.Fleets.Where(fleet => fleet.OwnedBy(player) && fleet.Aggregate.Colonizer);

            foreach (var fleet in colonizerFleets)
            {
                if (fleet.Waypoints.Count > 1 && fleet.Waypoints[1].Target is Planet planet)
                {
                    // don't send a colony ship to this planet
                    colonizablePlanets.Remove(planet);
                }
            }

            // go through each unassigned colonizer fleet and find it a new planet to colonize
            foreach (var fleet in colonizerFleets.Where(f => f.Waypoints.Count == 1 && f.Orbiting?.Player == player && f.Orbiting.GetPopulationDensity(settings) >= PopulationDensityRequired))
            {

                Planet planetToColonize = ClosestPlanet(fleet, colonizablePlanets);
                if (planetToColonize != null)
                {
                    // add this planet as a waypoint
                    var wp0 = fleet.Waypoints[0];
                    wp0.Task = WaypointTask.Transport;
                    wp0.TransportTasks = new WaypointTransportTasks();
                    wp0.TransportTasks.Colonists.Action = WaypointTaskTransportAction.LoadAll;

                    // TODO: setup waypoint transfer tasks
                    fleet.Waypoints.Add(new Waypoint(planetToColonize, fleet.GetDefaultWarpFactor(), WaypointTask.Colonize));

                    // remove this planet from our list
                    colonizablePlanets.Remove(planetToColonize);
                }
            }

            BuildFleets(buildablePlanets, colonizablePlanets.Count, colonyShip);

        }

        /// <summary>
        /// Build any new colonizer fleets
        /// </summary>
        /// <param name="buildablePlanets"></param>
        /// <param name="numShipsNeeded"></param>
        /// <param name="colonyShip"></param>
        void BuildFleets(List<Planet> buildablePlanets, int numShipsNeeded, ShipDesign colonyShip)
        {
            if (colonyShip != null)
            {

                int queuedToBeBuilt = 0;

                List<Planet> planetsToBuildOn = new List<Planet>();
                foreach (Planet planet in buildablePlanets)
                {
                    bool isBuilding = false;
                    foreach (ProductionQueueItem item in planet.ProductionQueue.Items)
                    {
                        if (item.Design != null && item.Design.Aggregate.Colonizer)
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
                        planet.ProductionQueue.Items.Add(new ProductionQueueItem(QueueItemType.ShipToken, 1, colonyShip));
                        log.Debug($"Added scout ship to planet queue: {planet.Name}");
                        queuedToBeBuilt++;
                    }
                }

            }

        }
    }
}