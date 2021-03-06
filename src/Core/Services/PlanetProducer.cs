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
        /// <param name="planet"></param>
        /// <returns></returns>
        public int Build(Planet planet)
        {

            var rules = planet.Player.Rules;
            // allocate surface minerals + resources not going to research
            Cost allocated = new Cost(planet.Cargo.Ironium, planet.Cargo.Boranium, planet.Cargo.Germanium,
                                      planet.ResourcesPerYearAvailable);

            // add the production queue's last turn resources
            var queue = planet.ProductionQueue;
            allocated += queue.Allocated;

            // try and build each item in the queue, in order
            int index = 0;
            while (index < queue.Items.Count)
            {
                ProductionQueueItem item = queue.Items[index];
                Cost costPer = item.GetCostOfOne(rules, planet.Player.Race);
                int numBuilt = allocated / costPer;

                // log.debug('Building item: %s cost_per: %s allocated: %s num_build: %s', item,
                // cost_per, allocated, numBuilt)
                // If we can build some, but not all
                if (0 < numBuilt && numBuilt < item.quantity)
                {
                    // build however many we can
                    allocated = BuildItem(planet, item, numBuilt, allocated);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numBuilt;

                    if (!AutoBuildTypes.Contains(item.type))
                    {
                        // reduce the quantity
                        var itemAtIndex = queue.Items[index];
                        itemAtIndex.quantity = queue.Items[index].quantity - numBuilt;
                        queue.Items[index] = itemAtIndex;
                    }

                    // allocate the leftover resources to the remaining items
                    queue.Allocated = AllocateToQueue(costPer, allocated);
                    allocated -= queue.Allocated;
                }
                else if (numBuilt >= item.quantity)
                {
                    // only build the amount required
                    numBuilt = item.quantity;
                    allocated = BuildItem(planet, item, numBuilt, allocated);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numBuilt;

                    if (!AutoBuildTypes.Contains(item.type))
                    {
                        // remove this item from the queue
                        queue.Items.RemoveAt(index);
                        index--;
                    }
                    // we built this completely, wipe out the allocated amount
                    queue.Allocated = new Cost();
                    planet.ProductionQueue = queue;
                }
                else
                {
                    // allocate as many minerals/resources as we can to the queue
                    // and break out of the loop, no more building will take place
                    queue.Allocated = AllocateToQueue(costPer, allocated);
                    allocated -= queue.Allocated;
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
        /// <param name="rules"></param>
        /// <returns></returns>
        private Cost BuildItem(Planet planet, ProductionQueueItem item, int numBuilt, Cost allocated)
        {
            if (item.type == QueueItemType.Mine || item.type == QueueItemType.AutoMine)
            {
                planet.Mines += numBuilt;
                Message.Mine(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.Factory || item.type == QueueItemType.AutoFactory)
            {
                planet.Factories += numBuilt;
                Message.Factory(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.Defense || item.type == QueueItemType.AutoDefense)
            {
                planet.Defenses += numBuilt;
                Message.Defense(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.Alchemy || item.type == QueueItemType.AutoAlchemy)
            {
                // add the minerals back to our allocated amount
                allocated += new Cost(numBuilt, numBuilt, numBuilt, 0);
            }
            else if (item.type == QueueItemType.ShipToken)
            {
                BuildFleet(planet, item, numBuilt);
            }
            else if (item.type == QueueItemType.Starbase)
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
        /// <param name="rules"></param>
        private void BuildFleet(Planet planet, ProductionQueueItem item, int numBuilt)
        {
            planet.Player.Stats.NumFleetsBuilt++;
            planet.Player.Stats.NumTokensBuilt += numBuilt;
            String name = item.fleetName != null ? item.fleetName : $"Fleet #{planet.Player.Stats.NumFleetsBuilt}";
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
            fleet.Tokens.Add(new ShipToken(item.Design, item.quantity));
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet));
            planet.OrbitingFleets.Add(fleet);

            Message.FleetBuilt(planet.Player, item.Design, fleet, numBuilt);
            EventManager.PublishFleetBuiltEvent(fleet);

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
        private Cost AllocateToQueue(Cost costPer, Cost allocated)
        {
            double ironiumPerc = (costPer.Ironium > 0 ? (double)(allocated.Ironium) / costPer.Ironium : 100.0);
            double boraniumPerc = (costPer.Boranium > 0 ? (double)(allocated.Boranium) / costPer.Boranium : 100.0);
            double germaniumPerc = (costPer.Germanium > 0 ? (double)(allocated.Germanium) / costPer.Germanium : 100.0);
            double resourcesPerc = (costPer.Resources > 0 ? (double)(allocated.Resources) / costPer.Resources : 100.0);

            // figure out the lowest percentage
            double minPerc = Math.Min(ironiumPerc, Math.Min(boraniumPerc, Math.Min(germaniumPerc, resourcesPerc)));

            // allocate the lowest percentage of each cost
            var newAllocated = new Cost(
                (int)(costPer.Ironium * minPerc),
                (int)(costPer.Boranium * minPerc),
                (int)(costPer.Germanium * minPerc),
                (int)(costPer.Resources * minPerc)
            );

            // return the amount we allocate to the top queued item
            return newAllocated;
        }
    }
}