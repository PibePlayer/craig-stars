using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class PlanetProductionStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetProductionStep));
        private readonly PlanetService planetService;
        private readonly PlayerService playerService;
        private readonly FleetSpecService fleetSpecService;

        public PlanetProductionStep(IProvider<Game> gameProvider, PlanetService planetService, PlayerService playerService, FleetSpecService fleetSpecService) : base(gameProvider, TurnGenerationState.PlanetProductionStep)
        {
            this.planetService = planetService;
            this.playerService = playerService;
            this.fleetSpecService = fleetSpecService;
        }

        /// <summary>
        /// The result of processing a QueueItem during production
        /// </summary>
        internal struct ProcessItemResult
        {
            public readonly bool completed;
            public readonly int numCompleted;
            public readonly Cost remainingCost;

            public ProcessItemResult(bool completed, int numCompleted, Cost remainingCost)
            {
                this.completed = completed;
                this.numCompleted = numCompleted;
                this.remainingCost = remainingCost;
            }

            public override string ToString()
            {
                return $"completed: {completed}, numCompleted: {numCompleted}, remainingCost: {remainingCost}";
            }
        }

        public override void Process()
        {
            Game.Players.ForEach(player => player.LeftoverResources = 0);
            OwnedPlanets.ForEach(planet =>
            {
                int leftoverResources = Build(planet, Game.Players[planet.PlayerNum]);
                Game.Players[planet.PlayerNum].LeftoverResources += leftoverResources;
            });
        }

        /// <summary>
        /// Build anything in the production queue on the planet.
        /// </summary>
        /// <param name="planet"></param>
        /// <returns></returns>
        public int Build(Planet planet, Player player)
        {
            // allocate surface minerals + resources not going to research
            Cost allocated = new Cost(planet.Cargo.Ironium, planet.Cargo.Boranium, planet.Cargo.Germanium,
                                      planet.Spec.ResourcesPerYearAvailable);

            // add the production queue's last turn resources
            var queue = planet.ProductionQueue;

            // try and build each item in the queue, in order
            int index = 0;
            while (index != -1 && index < queue.Items.Count)
            {
                var item = queue.Items[index];
                var result = ProcessItem(planet, player, item, allocated);
                allocated = result.remainingCost;
                if (result.completed)
                {
                    // we completed this item
                    // if it's an auto item, leave it in the queue
                    // otherwise remove it and move on
                    if (!item.IsAuto)
                    {
                        // we completed this normal item. Remove it from the queue
                        queue.Items.RemoveAt(index);
                        continue;
                    }
                }
                else
                {
                    // we didn't finish this item, but we made some progress
                    if (item.IsAuto)
                    {
                        // if this item is an auto build and we don't have resources leftover
                        // queue up a partially built item and break out of production, we are done
                        if (result.remainingCost.Resources <= 0)
                        {
                            // we completed one or more auto build items, so add the remaining
                            // to the top of the queue and break
                            QueueItemType realItemTypeToAdd = item.Type switch
                            {
                                QueueItemType.AutoMines => QueueItemType.Mine,
                                QueueItemType.AutoFactories => QueueItemType.Factory,
                                QueueItemType.AutoDefenses => QueueItemType.Defenses,
                                QueueItemType.AutoMaxTerraform => QueueItemType.TerraformEnvironment,
                                QueueItemType.AutoMinTerraform => QueueItemType.TerraformEnvironment,
                                QueueItemType.AutoMineralAlchemy => QueueItemType.MineralAlchemy,
                                QueueItemType.AutoMineralPacket => QueueItemType.MixedMineralPacket,
                                _ => throw new ArgumentException($"No regular item corresponds to {item.Type}")
                            };
                            // add this partial item to the top of the queue
                            queue.Items.Insert(0, new ProductionQueueItem(realItemTypeToAdd, 1, allocated: item.Allocated));
                            item.Allocated = new Cost();
                            break;
                        }
                        else
                        {
                            // We still have resources leftover. Reclaim any resources
                            // we spent on this and use it on the next item.
                            allocated += item.Allocated;
                            item.Allocated = new Cost();
                        }
                    }
                    else
                    {
                        // this wasn't an auto and we didnt' finish, the production is done
                        break;
                    }
                }

                // if we got here, we are moving onto the next one
                index++;
            }

            // reset surface minerals to whatever we have leftover
            planet.Cargo = new Cargo(allocated.Ironium, allocated.Boranium, allocated.Germanium, planet.Cargo.Colonists);

            // return the resources we have left for research
            return allocated.Resources;

        }

        /// <summary>
        /// Process an item in the production queue
        /// </summary>
        /// <param name="planet">The planet with the item</param>
        /// <param name="item">The item to process</param>
        /// <param name="allocated">The amount of resources and minerals we have to allocate to this item</param>
        /// <returns></returns>
        internal ProcessItemResult ProcessItem(Planet planet, Player player, ProductionQueueItem item, Cost allocated)
        {
            if (item.IsPacket && !planet.HasMassDriver)
            {
                // can't built, break out
                Message.BuildMineralPacketNoMassDriver(player, planet);
                return new ProcessItemResult(completed: true, item.Quantity, allocated);
            }
            if (item.IsPacket && planet.PacketTarget == null)
            {
                // can't built, break out
                Message.BuildMineralPacketNoTarget(player, planet);
                return new ProcessItemResult(completed: true, item.Quantity, allocated);
            }

            // no need to build anymore of this, skip it.
            int quantityDesired = planetService.GetQuantityToBuild(planet, player, item.Quantity, item.Type);
            if (quantityDesired == 0)
            {
                return new ProcessItemResult(completed: true, item.Quantity, allocated);
            }

            // add anything we've already allocated to this item
            allocated += item.Allocated;

            Cost costPer = playerService.GetCostOfOne(player, item);
            if (item.Type == QueueItemType.Starbase && planet.HasStarbase)
            {
                costPer = planet.Starbase.GetUpgradeCost(item.Design);
            }
            int numBuilt = Math.Min(quantityDesired, (int)(allocated / costPer));

            // build however many we can
            if (item.IsMineralAlchemy)
            {
                allocated += BuildMineralAlchemy(planet, numBuilt);
            }
            else
            {
                if (numBuilt > 0)
                {
                    BuildItem(planet, player, item, numBuilt);
                }
            }

            // remove this cost from our allocated amount
            allocated -= costPer * numBuilt;

            if (!item.IsAuto)
            {
                // reduce the quantity
                item.Quantity = item.Quantity - numBuilt;
            }

            // If we can build some, but not all
            if (numBuilt > 0 && numBuilt < quantityDesired)
            {
                // we finished some of these items, but not all
                item.Allocated = AllocatePartialBuild(costPer, allocated);
                allocated -= item.Allocated;
                return new ProcessItemResult(completed: false, numBuilt, allocated);
            }
            else if (numBuilt >= quantityDesired)
            {
                // all done with this item
                return new ProcessItemResult(completed: true, numBuilt, allocated);
            }
            else
            {
                // allocate as many minerals/resources as we can to the this item
                item.Allocated = AllocatePartialBuild(costPer, allocated);
                allocated -= item.Allocated;
                return new ProcessItemResult(completed: false, numBuilt, allocated);
            }

        }

        /// <summary>
        /// Build 1 or more items of this production queue item type Adding mines, factories, defenses,
        /// etc to planets Building new fleets
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        internal void BuildItem(Planet planet, Player player, ProductionQueueItem item, int numBuilt)
        {
            if (item.Type == QueueItemType.Mine || item.Type == QueueItemType.AutoMines)
            {
                log.Debug($"{Game.Year}: {planet.PlayerNum} built {numBuilt} mines on {planet.Name}");
                planet.Mines += numBuilt;
                // this should never need to clamp because we adjust quantity in Build(), but just in case
                planet.Mines = Mathf.Clamp(planet.Mines, 0, planet.Spec.MaxPossibleMines);
                Message.Mine(player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Factory || item.Type == QueueItemType.AutoFactories)
            {
                log.Debug($"{Game.Year}: {player} built {numBuilt} factories on {planet.Name}");
                planet.Factories += numBuilt;
                planet.Factories = Mathf.Clamp(planet.Factories, 0, planet.Spec.MaxPossibleFactories);
                Message.Factory(player, planet, numBuilt);
            }
            else if (item.Type == QueueItemType.Defenses || item.Type == QueueItemType.AutoDefenses)
            {
                log.Debug($"{Game.Year}: {player} built {numBuilt} defenses on {planet.Name}");
                planet.Defenses += numBuilt;
                planet.Defenses = Mathf.Clamp(planet.Defenses, 0, planet.Spec.MaxDefenses);
                Message.Defense(player, planet, numBuilt);
            }
            else if (item.IsTerraform)
            {
                for (int i = 0; i < numBuilt; i++)
                {
                    // terraform one at a time to ensure the best things get terraformed
                    (HabType habType, int direction) = planetService.TerraformOneStep(planet, player);
                    Message.Terraform(player, planet, habType, direction);
                }
            }
            else if (item.IsPacket)
            {
                BuildPacket(planet, player, playerService.GetCostOfOne(player, item), numBuilt);
            }
            else if (item.Type == QueueItemType.ShipToken)
            {
                BuildFleet(planet, player, item, numBuilt);
            }
            else if (item.Type == QueueItemType.Starbase)
            {
                BuildStarbase(planet, player, item);
            }

        }

        /// <summary>
        /// Build 1 or more items of this production queue item type Adding mines, factories, defenses,
        /// etc to planets Building new fleets
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        internal Cost BuildMineralAlchemy(Planet planet, int numBuilt)
        {
            // add the minerals back to our allocated amount
            return new Cost(numBuilt, numBuilt, numBuilt, 0);
        }

        internal void BuildPacket(Planet planet, Player player, Cargo cargo, int numBuilt)
        {
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = planet.PlayerNum,
                Position = planet.Position,
                SafeWarpSpeed = planet.Starbase.Spec.SafePacketSpeed,
                WarpFactor = planet.PacketSpeed,
                Target = planet.PacketTarget,
                Cargo = cargo * numBuilt
            };

            Message.MineralPacket(player, planet, packet);
            EventManager.PublishMapObjectCreatedEvent(packet);
        }

        /// <summary>
        /// Build a fleet and add it to the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <param name="numBuilt"></param>
        /// <param name="rules"></param>
        internal void BuildFleet(Planet planet, Player player, ProductionQueueItem item, int numBuilt)
        {
            player.Stats.NumFleetsBuilt++;
            player.Stats.NumTokensBuilt += numBuilt;
            var id = player.Stats.NumFleetsBuilt;
            string name = item.FleetName != null ? item.FleetName : item.Design.Name;
            var existingFleetByName = planet.OrbitingFleets.Where(f => f.Name == name);
            var existingFleetsRequiringTokens = planet.OrbitingFleets.Where(f => !f.Spec.FleetCompositionComplete).ToList();

            bool foundFleet = false;
            if (existingFleetsRequiringTokens.Count > 0)
            {
                foreach (Fleet fleet in existingFleetsRequiringTokens)
                {
                    if (fleet.FleetComposition.GetQuantityByPurpose().ContainsKey(item.Design.Purpose))
                    {
                        var existingToken = fleet.Tokens.Find(token => token.Design == item.Design);
                        if (existingToken != null)
                        {
                            existingToken.Quantity += numBuilt;
                        }
                        else
                        {
                            // this fleet composition requires a ShipDesign like this, so add it to the fleet
                            fleet.Tokens.Add(new ShipToken(item.Design, item.Quantity));
                        }

                        fleetSpecService.ComputeFleetSpec(player, fleet, true);
                        Message.FleetBuiltForComposition(player, item.Design, fleet, numBuilt);
                        foundFleet = true;
                        break;
                    }
                }
            }

            if (!foundFleet)
            {
                Fleet fleet = new Fleet()
                {
                    BaseName = name,
                    Name = $"{name} #{id}",
                    RaceName = player.Race.Name,
                    RacePluralName = player.Race.PluralName,
                    PlayerNum = planet.PlayerNum,
                    Orbiting = planet,
                    Position = planet.Position,
                    Id = id,
                    BattlePlan = player.BattlePlans[0]
                };
                fleet.Tokens.Add(new ShipToken(item.Design, item.Quantity));
                fleetSpecService.ComputeFleetSpec(player, fleet);
                fleet.Fuel = fleet.Spec.FuelCapacity;
                fleet.Waypoints.Add(Waypoint.TargetWaypoint(planet, warpFactor: fleet.Spec.Engine.IdealSpeed));
                planet.OrbitingFleets.Add(fleet);

                Message.FleetBuilt(player, item.Design, fleet, numBuilt);
                EventManager.PublishMapObjectCreatedEvent(fleet);
            }

        }

        /// <summary>
        /// Build or upgrade the starbase on the planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        internal void BuildStarbase(Planet planet, Player player, ProductionQueueItem item)
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
                    PlayerNum = planet.PlayerNum,
                    Orbiting = planet,
                    Position = planet.Position,
                    BattlePlan = player.BattlePlans[0]
                };
            }
            planet.Starbase.Name = item.Design.Name;
            planet.Starbase.Tokens.Add(new ShipToken(item.Design, 1));
            fleetSpecService.ComputeStarbaseSpec(player, planet.Starbase, true);
            planet.PacketSpeed = planet.Starbase.Spec.SafePacketSpeed;
            Message.FleetBuilt(player, item.Design, planet.Starbase, 1);

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
        /// <param name="costPerItem"></param>
        /// <param name="allocated"></param>
        /// <returns></returns>
        internal Cost AllocatePartialBuild(Cost costPerItem, Cost allocated)
        {
            double ironiumPerc = (costPerItem.Ironium > 0 ? (double)(allocated.Ironium) / costPerItem.Ironium : 100.0);
            double boraniumPerc = (costPerItem.Boranium > 0 ? (double)(allocated.Boranium) / costPerItem.Boranium : 100.0);
            double germaniumPerc = (costPerItem.Germanium > 0 ? (double)(allocated.Germanium) / costPerItem.Germanium : 100.0);
            double resourcesPerc = (costPerItem.Resources > 0 ? (double)(allocated.Resources) / costPerItem.Resources : 100.0);

            // figure out the lowest percentage
            double minPerc = Math.Min(ironiumPerc, Math.Min(boraniumPerc, Math.Min(germaniumPerc, resourcesPerc)));

            // allocate the lowest percentage of each cost
            var newAllocated = new Cost(
                (int)(costPerItem.Ironium * minPerc),
                (int)(costPerItem.Boranium * minPerc),
                (int)(costPerItem.Germanium * minPerc),
                (int)(costPerItem.Resources * minPerc)
            );

            // return the amount we allocate to the top queued item
            return newAllocated;
        }


    }
}