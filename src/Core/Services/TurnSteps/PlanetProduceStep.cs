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

            // try and build each item in the queue, in order
            int index = 0;
            while (index < queue.Items.Count)
            {
                ProductionQueueItem item = queue.Items[index];

                if (item.IsPacket && !planet.HasMassDriver)
                {
                    // can't built, break out
                    Message.BuildMineralPacketNoMassDriver(planet.Player, planet);
                    index++;
                    continue;
                }
                if (item.IsPacket && planet.PacketTarget == null)
                {
                    // can't built, break out
                    Message.BuildMineralPacketNoTarget(planet.Player, planet);
                    index++;
                    continue;
                }

                // no need to build anymore of this, skip it.
                int quantityDesired = planet.GetQuantityToBuild(item.Quantity, item.Type);
                if (quantityDesired == 0)
                {
                    index++;
                    continue;
                }

                // add anything we've already allocated to this item
                allocated += item.Allocated;

                Cost costPer = item.GetCostOfOne(rules, planet.Player);
                if (item.Type == QueueItemType.Starbase && planet.HasStarbase)
                {
                    costPer = planet.Starbase.GetUpgradeCost(item.Design);
                }
                int numAbleToBeBuilt = (int)(allocated / costPer);

                // If we can build some, but not all
                if (numAbleToBeBuilt > 0 && numAbleToBeBuilt < quantityDesired)
                {
                    // build however many we can
                    allocated = BuildItem(planet, item, numAbleToBeBuilt, allocated);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numAbleToBeBuilt;

                    if (!item.IsAuto)
                    {
                        // reduce the quantity
                        item.Quantity = item.Quantity - numAbleToBeBuilt;
                    }

                    // allocate the leftover resources to the remaining items
                    item.Allocated = AllocateToQueue(costPer, allocated);
                    allocated -= item.Allocated;
                }
                else if (numAbleToBeBuilt >= quantityDesired)
                {
                    // only build the amount required
                    numAbleToBeBuilt = quantityDesired;
                    allocated = BuildItem(planet, item, numAbleToBeBuilt, allocated);

                    // remove this cost from our allocated amount
                    allocated -= costPer * numAbleToBeBuilt;

                    if (!item.IsAuto)
                    {
                        // remove this item from the queue
                        queue.Items.RemoveAt(index);
                        index--;
                    }
                    // we built this completely, wipe out the allocated amount
                    planet.ProductionQueue = queue;
                }
                else
                {
                    // allocate as many minerals/resources as we can to the queue
                    // and break out of the loop, no more building will take place
                    item.Allocated = AllocateToQueue(costPer, allocated);
                    allocated -= item.Allocated;
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
            if (item.Type == QueueItemType.Mine || item.Type == QueueItemType.AutoMines)
            {
                planet.Mines += numBuilt;
                // this should never need to clamp because we adjust quantity in Build(), but just in case
                planet.Mines = Mathf.Clamp(planet.Mines, 0, planet.MaxPossibleMines);
                Message.Mine(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Factory || item.Type == QueueItemType.AutoFactories)
            {
                planet.Factories += numBuilt;
                planet.Factories = Mathf.Clamp(planet.Factories, 0, planet.MaxPossibleFactories);
                Message.Factory(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Defenses || item.Type == QueueItemType.AutoDefenses)
            {
                planet.Defenses += numBuilt;
                planet.Defenses = Mathf.Clamp(planet.Defenses, 0, planet.MaxDefenses);
                Message.Defense(planet.Player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.MineralAlchemy || item.Type == QueueItemType.AutoMineralAlchemy)
            {
                // add the minerals back to our allocated amount
                allocated += new Cost(numBuilt, numBuilt, numBuilt, 0);
            }
            else if (item.IsTerraform)
            {
                for (int i = 0; i < numBuilt; i++)
                {
                    // terraform one at a time to ensure the best things get terraformed
                    Terraform(planet);
                }
            }
            else if (item.IsPacket)
            {
                BuildPacket(planet, item.GetCostOfOne(Game.Rules, planet.Player), numBuilt);
            }
            else if (item.Type == QueueItemType.ShipToken)
            {
                BuildFleet(planet, item, numBuilt);
            }
            else if (item.Type == QueueItemType.Starbase)
            {
                BuildStarbase(planet, item);
            }

            return allocated;
        }

        void BuildPacket(Planet planet, Cargo cargo, int numBuilt)
        {
            MineralPacket packet = new MineralPacket()
            {
                Player = planet.Player,
                Position = planet.Position,
                WarpFactor = planet.PacketSpeed,
                Target = planet.PacketTarget,
                Cargo = cargo * numBuilt
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
            string name = item.FleetName != null ? item.FleetName : $"{item.Design.Name} #{id}";
            var existingFleet = planet.OrbitingFleets.Where(f => f.Name == name);

            Fleet fleet = new Fleet()
            {
                Name = name,
                Player = planet.Player,
                Orbiting = planet,
                Position = planet.Position,
                Id = id,
                BattlePlan = planet.Player.BattlePlans[0]
            };
            fleet.Tokens.Add(new ShipToken(item.Design, item.Quantity));
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet));
            planet.OrbitingFleets.Add(fleet);

            Message.FleetBuilt(planet.Player, item.Design, fleet, numBuilt);
            EventManager.PublishMapObjectCreatedEvent(fleet);
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
            Message.FleetBuilt(planet.Player, item.Design, planet.Starbase, 1);

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

        /// <summary>
        /// Terraform this planet one step in whatever the best option is
        /// </summary>
        /// <param name="planet"></param>
        public void Terraform(Planet planet)
        {
            var bestHab = planet.GetBestTerraform();
            if (bestHab.HasValue)
            {
                var habType = bestHab.Value;
                int fromIdeal = planet.Player.Race.HabCenter[habType] - planet.Hab.Value[habType];
                if (fromIdeal > 0)
                {
                    // for example, the planet has Grav 49, but our player wants Grav 50 
                    planet.Hab = planet.Hab.Value.WithType(habType, planet.Hab.Value[habType] + 1);
                    Message.Terraform(planet.Player, planet, habType, 1);
                }
                else if (fromIdeal < 1)
                {
                    // for example, the planet has Grav 51, but our player wants Grav 50 
                    planet.Hab = planet.Hab.Value.WithType(habType, planet.Hab.Value[habType] - 1);
                    Message.Terraform(planet.Player, planet, habType, -1);
                }
            }
        }
    }
}