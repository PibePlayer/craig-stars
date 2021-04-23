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
        public PlayerPlansGenerationStep(Game game) : base(game, UniverseGenerationState.Plans) { }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                player.BattlePlans = GetBattlePlans();
                player.TransportPlans = GetTransportPlans();
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
    }
}