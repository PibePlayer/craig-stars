using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class PlanetProduceStep : TurnGenerationStep
    {
        static HashSet<QueueItemType> AutoBuildTypes = new HashSet<QueueItemType>() {
            QueueItemType.AutoMineralAlchemy,
            QueueItemType.AutoMines,
            QueueItemType.AutoDefenses,
            QueueItemType.AutoFactories,
            QueueItemType.AutoMineralPacket
        };

        public PlanetProduceStep(Game game) : base(game, TurnGenerationState.Production) { }

        public override void Process()
        {
            OwnedPlanets.ForEach(planet =>
            {
                Build(planet);
            });
        }

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

                if (item.IsPacket && !planet.HasMassDriver)
                {
                    // can't built, break out
                    Message.BuildMineralPacketNoMassDriver(planet.Player, planet);
                    break;
                }
                if (item.IsPacket && planet.PacketTarget == null)
                {
                    // can't built, break out
                    Message.BuildMineralPacketNoTarget(planet.Player, planet);
                    break;
                }

                Cost costPer = item.GetCostOfOne(rules, planet.Player);
                if (item.type == QueueItemType.Starbase && planet.HasStarbase)
                {
                    costPer = planet.Starbase.GetUpgradeCost(item.Design);
                }
                int numBuilt = (int)(allocated / costPer);

                // if we are autobuilding, don't build more than our max
                // i.e. if we have 95 mines, 100 max mines, and we are autobuilding 10 mines
                // we should only actually autobuild 5 mines
                int quantityDesired = item.quantity;
                switch (item.type)
                {
                    case QueueItemType.AutoMines:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxMines - planet.Mines);
                        break;
                    case QueueItemType.AutoFactories:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxFactories - planet.Factories);
                        break;
                    case QueueItemType.AutoDefenses:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxDefenses - planet.Defenses);
                        break;
                    case QueueItemType.Mine:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxPossibleMines - planet.Mines);
                        break;
                    case QueueItemType.Factory:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxPossibleFactories - planet.Factories);
                        break;
                    case QueueItemType.Defenses:
                        quantityDesired = Math.Min(quantityDesired, planet.MaxDefenses - planet.Defenses);
                        break;
                }

                // no need to build anymore
                if (quantityDesired <= 0)
                {
                    index++;
                    continue;
                }

                // log.debug('Building item: %s cost_per: %s allocated: %s num_build: %s', item,
                // cost_per, allocated, numBuilt)
                // If we can build some, but not all
                if (0 < numBuilt && numBuilt < quantityDesired)
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
                else if (numBuilt >= quantityDesired)
                {
                    // only build the amount required
                    numBuilt = quantityDesired;
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
            planet.Cargo = new Cargo(allocated.Ironium, allocated.Boranium, allocated.Germanium, planet.Cargo.Colonists);

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
            if (item.type == QueueItemType.Mine || item.type == QueueItemType.AutoMines)
            {
                // TODO: return resources from overbuild back to player
                planet.Mines += numBuilt;
                planet.Mines = Mathf.Clamp(planet.Mines, 0, planet.MaxPossibleMines);
                Message.Mine(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.Factory || item.type == QueueItemType.AutoFactories)
            {
                planet.Factories += numBuilt;
                planet.Factories = Mathf.Clamp(planet.Factories, 0, planet.MaxPossibleFactories);
                Message.Factory(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.Defenses || item.type == QueueItemType.AutoDefenses)
            {
                planet.Defenses += numBuilt;
                planet.Defenses = Mathf.Clamp(planet.Defenses, 0, planet.MaxDefenses);
                Message.Defense(planet.Player, planet, numBuilt);
            }
            else if (item.type == QueueItemType.MineralAlchemy || item.type == QueueItemType.AutoMineralAlchemy)
            {
                // add the minerals back to our allocated amount
                allocated += new Cost(numBuilt, numBuilt, numBuilt, 0);
            }
            else if (
                item.type == QueueItemType.IroniumMineralPacket ||
                item.type == QueueItemType.BoraniumMineralPacket ||
                item.type == QueueItemType.GermaniumMineralPacket ||
                item.type == QueueItemType.MixedMineralPacket ||
                item.type == QueueItemType.AutoMineralPacket
            )
            {
                BuildPacket(planet, item.type, numBuilt);
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

        void BuildPacket(Planet planet, QueueItemType item, int numBuilt)
        {
            MineralPacket packet = new MineralPacket()
            {
                Player = planet.Player,
                Position = planet.Position,
                WarpFactor = planet.PacketSpeed,
                Target = planet.PacketTarget
            };

            Message.MineralPacket(planet.Player, planet, packet);
            EventManager.PublishMapObjectCreatedEvent(packet);
        }

        /// <summary>
        /// Build a fleet and add it to the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="rules"></param>
        void BuildFleet(Planet planet, ProductionQueueItem item, int numBuilt)
        {
            planet.Player.Stats.NumFleetsBuilt++;
            planet.Player.Stats.NumTokensBuilt += numBuilt;
            var id = planet.Player.Stats.NumFleetsBuilt;
            string name = item.fleetName != null ? item.fleetName : $"{item.Design.Name} #{id}";
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
                Position = planet.Position,
                Id = id,
                BattlePlan = planet.Player.BattlePlans[0]
            };
            fleet.Tokens.Add(new ShipToken(item.Design, item.quantity));
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet));
            planet.OrbitingFleets.Add(fleet);

            Message.FleetBuilt(planet.Player, item.Design, fleet, numBuilt);
            EventManager.PublishMapObjectCreatedEvent(fleet);

            // }
        }

        /// <summary>
        /// Build or upgrade the starbase on the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        void BuildStarbase(Planet planet, ProductionQueueItem item)
        {
            if (planet.Starbase != null)
            {
                // remove the existing starbase
                planet.Starbase.Tokens.Clear();
            }
            else
            {
                planet.Starbase = new Starbase()
                {
                    Player = planet.Player,
                    Orbiting = planet,
                    Position = planet.Position,
                    BattlePlan = planet.Player.BattlePlans[0]
                };
            }
            planet.Starbase.Name = item.Design.Name;
            planet.Starbase.Tokens.Add(new ShipToken(item.Design, 1));
            planet.Starbase.ComputeAggregate(true);
            // Message.StarbaseBuilt(planet.Player, item.Design, planet);

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
        Cost AllocateToQueue(Cost costPer, Cost allocated)
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