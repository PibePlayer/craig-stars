using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;

namespace CraigStars
{
    public class PlanetProducer
    {
        public static HashSet<QueueItemType> AutoBuildTypes = new HashSet<QueueItemType>() {
            QueueItemType.AutoAlchemy,
            QueueItemType.AutoMine,
            QueueItemType.AutoDefense,
            QueueItemType.AutoFactory
        };

        /// <summary>
        /// Build anything in the production queue on the planet.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="planet"></param>
        /// <returns></returns>
        public int Build(UniverseSettings settings, Planet planet)
        {

            // allocate surface minerals + resources not going to research
            Cost allocated = new Cost(planet.Cargo.Ironium, planet.Cargo.Boranium, planet.Cargo.Germanium,
                                      planet.ResourcesPerYearAvailable);

            // add the production queue's last turn resources
            allocated += planet.ProductionQueue.Allocated;

            // try and build each item in the queue, in order
            int index = 0;
            while (index < planet.ProductionQueue.Items.Count)
            {
                ProductionQueueItem item = planet.ProductionQueue.Items[index];
                Cost costPer = item.GetCostOfOne(settings, planet.Player.Race);
                int numBuilt = allocated / costPer;

                // log.debug('Building item: %s cost_per: %s allocated: %s num_build: %s', item,
                // cost_per, allocated, numBuilt)
                // If we can build some, but not all
                if (0 < numBuilt && numBuilt < item.Quantity)
                {
                    // build however many we can
                    allocated = BuildItem(planet, item, numBuilt, allocated, settings);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numBuilt;

                    if (!AutoBuildTypes.Contains(item.Type))
                    {
                        // reduce the quantity
                        planet.ProductionQueue.Items[index].Quantity = planet.ProductionQueue.Items[index].Quantity - numBuilt;
                    }

                    // allocate the leftover resources to the remaining items
                    allocated = AllocateToQueue(planet.ProductionQueue, costPer, allocated);
                }
                else if (numBuilt >= item.Quantity)
                {
                    // only build the amount required
                    numBuilt = item.Quantity;
                    allocated = BuildItem(planet, item, numBuilt, allocated, settings);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numBuilt;

                    if (!AutoBuildTypes.Contains(item.Type))
                    {
                        // remove this item from the queue
                        planet.ProductionQueue.Items.RemoveAt(index);
                        index--;
                    }
                    // we built this completely, wipe out the allocated amount
                    planet.ProductionQueue.Allocated = new Cost();
                }
                else
                {
                    // allocate as many minerals/resources as we can to the queue
                    // and break out of the loop, no more building will take place
                    allocated = AllocateToQueue(planet.ProductionQueue, costPer, allocated);
                    break;
                }
                index++;
            }

            // reset surface minerals to whatever we have leftover
            planet.Cargo = new Cargo(allocated.Ironium, allocated.Boranium, allocated.Germanium, planet.Cargo.Colonists, planet.Cargo.Fuel);

            // return the resources we have left for research
            return allocated.Resources;

        }

        /// <summary>
        /// Build 1 or more items of this production queue item type Adding mines, factories, defenses,
        /// etc to planets Building new fleets
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="allocated"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private Cost BuildItem(Planet planet, ProductionQueueItem item, int numBuilt, Cost allocated, UniverseSettings settings)
        {
            if (item.Type == QueueItemType.Mine || item.Type == QueueItemType.AutoMine)
            {
                planet.Mines += numBuilt;
                Message.Mine(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Factory || item.Type == QueueItemType.AutoFactory)
            {
                planet.Factories += numBuilt;
                Message.Factory(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Defense || item.Type == QueueItemType.AutoDefense)
            {
                planet.Defenses += numBuilt;
                Message.Defense(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Alchemy || item.Type == QueueItemType.AutoAlchemy)
            {
                // add the minerals back to our allocated amount
                allocated += new Cost(numBuilt, numBuilt, numBuilt, 0);
            }
            else if (item.Type == QueueItemType.ShipToken)
            {
                BuildFleet(planet, item, numBuilt, settings);
            }
            else if (item.Type == QueueItemType.Starbase)
            {
                BuildStarbase(planet, item);
            }

            return allocated;
        }

        /// <summary>
        /// Build a fleet and add it to the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="settings"></param>
        private void BuildFleet(Planet planet, ProductionQueueItem item, int numBuilt, UniverseSettings settings)
        {
            planet.Player.Stats.NumFleetsBuilt++;
            planet.Player.Stats.NumTokensBuilt += numBuilt;
            String name = item.FleetName != null ? item.FleetName : $"Fleet #{planet.Player.Stats.NumFleetsBuilt}";
            var existingFleet = planet.OrbitingFleets.Where(f => f.Name == name);

            // if (we didn't have a fleet of that name, or it wasn't defined
            // just add this fleet as it's own entity
            // if (existingFleet != null)
            // {
            //     // merge this fleet into an existing fleet
            //     // existingFleet.Merge(item.Design, numBuilt);
            // }
            // else
            // {
            Fleet fleet = new Fleet()
            {
                Name = name,
                Player = planet.Player,
                Orbiting = planet,
                Position = planet.Position
            };
            fleet.Tokens.Add(new ShipToken(item.Design, item.Quantity));
            fleet.ComputeAggregate(settings);
            fleet.Fuel = fleet.Aggregate.FuelCapacity;
            fleet.Waypoints.Add(new Waypoint(planet));
            planet.OrbitingFleets.Add(fleet);
            Message.FleetBuilt(planet.Player, item.Design, fleet, numBuilt);

            Signals.PublishFleetBuiltEvent(fleet);

            // }
        }

        /// <summary>
        /// Build or upgrade the starbase on the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        private void BuildStarbase(Planet planet, ProductionQueueItem item)
        {
            // if (planet.Starbase != null)
            // {
            //     // upgrade the existing starbase
            //     planet.Starbase().getShipStacks().clear;
            //     planet.Starbase().getShipStacks().add(new ShipStack(item.getShipDesign, 1));
            //     planet.Starbase.setDamage(0);
            //     planet.Starbase().computeAggregate;
            // }
            // else
            // {
            //     // create a new starbase
            //     Fleet fleet = fleetController.create(planet.Name() + "-starbase", planet.getX(), planet.getY(), planet.getOwner);
            //     fleet.ShipStacks().add(new ShipStack(item.getShipDesign, 1));
            //     fleet.computeAggregate();
            //     fleet.addWaypoint(planet.X(), planet.getY, 5, WaypointTask.None, planet);
            //     planet.setStarbase(fleet);
            //     planet.Player().getGame().getFleets.add(fleet);
            // }
        }

        /// <summary>
        /// Allocate resources to the top item on this production queue
        /// and return the leftover resources
        ///  
        /// Costs are allocated by lowest percentage, i.e. if (we require
        /// Cost(10, 10, 10, 100) and we only have Cost(1, 10, 10, 100)
        /// we allocate Cost(1, 1, 1, 10)
        ///  
        /// The min amount we have is 10 percent of the ironium, so we
        /// apply 10 percent to each cost amount
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="costPer"></param>
        /// <param name="allocated"></param>
        /// <returns></returns>
        private Cost AllocateToQueue(ProductionQueue queue, Cost costPer, Cost allocated)
        {
            double ironiumPerc = (costPer.Ironium > 0 ? (double)(allocated.Ironium) / costPer.Ironium : 100.0);
            double boraniumPerc = (costPer.Boranium > 0 ? (double)(allocated.Boranium) / costPer.Boranium : 100.0);
            double germaniumPerc = (costPer.Germanium > 0 ? (double)(allocated.Germanium) / costPer.Germanium : 100.0);
            double resourcesPerc = (costPer.Resources > 0 ? (double)(allocated.Resources) / costPer.Resources : 100.0);

            // figure out the lowest percentage
            double minPerc = Math.Min(ironiumPerc, Math.Min(boraniumPerc, Math.Min(germaniumPerc, resourcesPerc)));

            // allocate the lowest percentage of each cost
            queue.Allocated = new Cost(
                (int)(costPer.Ironium * minPerc),
                (int)(costPer.Boranium * minPerc),
                (int)(costPer.Germanium * minPerc),
                (int)(costPer.Resources * minPerc)
            );

            // return the leftovers
            return allocated -= queue.Allocated;
        }
    }
}