using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Create defuault battle plans and transport plans for players
    /// </summary>
    public class PlayerPlansGenerationStep : UniverseGenerationStep
    {
        readonly PlanetService planetService;

        public PlayerPlansGenerationStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, UniverseGenerationState.Plans)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                player.BattlePlans = GetBattlePlans();
                player.TransportPlans = GetTransportPlans();
                player.ProductionPlans = GetProductionPlans();
                player.SetupMapObjectMappings();
            });
        }

        internal List<BattlePlan> GetBattlePlans()
        {
            return new List<BattlePlan>() {
                new BattlePlan("Default")
            };
        }

        internal List<TransportPlan> GetTransportPlans()
        {
            return new List<TransportPlan>() {
                new TransportPlan("Default"),
                new TransportPlan("Quick Load")
                {
                    Tasks = new WaypointTransportTasks(
                        fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal),
                        ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll),
                        boranium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll),
                        germanium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll)
                        )
                },
                new TransportPlan("Quick Drop")
                {
                    Tasks = new WaypointTransportTasks(
                        fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal),
                        ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll),
                        boranium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll),
                        germanium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll)
                        )
                },
                new TransportPlan("Load Colonists")
                {
                    Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll))
                },
                new TransportPlan("Unload Colonists")
                {
                    Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll))
                },
            };
        }

        internal List<ProductionPlan> GetProductionPlans()
        {
            return new()
            {
                new ProductionPlan("Default")
                {
                    Items = {
                        new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 1),
                        new ProductionQueueItem(QueueItemType.AutoFactories, 5),
                        new ProductionQueueItem(QueueItemType.AutoMines, 5),
                    }
                }
            };
        }

    }
}