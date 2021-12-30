
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PlayerOrders
    {
        public int PlayerNum { get; set; }
        public string Token { get; set; }
        public PlayerUISettings UISettings { get; set; } = new();
        public PlayerSettings Settings { get; set; } = new();

        public List<PlayerRelationship> PlayerRelations { get; set; } = new();

        public PlayerResearchOrder Research { get; set; } = new();
        public List<FleetWaypointsOrders> FleetOrders { get; set; } = new();
        public List<PlanetProductionOrder> PlanetProductionOrders { get; set; } = new();
        public List<ShipDesign> ShipDesigns { get; set; } = new();
        public List<MineFieldOrder> MineFieldOrders { get; set; } = new();

        public List<BattlePlan> BattlePlans { get; set; } = new();
        public List<TransportPlan> TransportPlans { get; set; } = new();
        public List<ProductionPlan> ProductionPlans { get; set; } = new();
        public List<FleetComposition> FleetCompositions { get; set; } = new();

        public List<ImmediateFleetOrder> ImmedateFleetOrders { get; set; } = new();
    }
}