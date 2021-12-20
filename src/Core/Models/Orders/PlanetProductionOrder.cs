
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PlanetProductionOrder : PlayerObjectOrder
    {
        public Guid? PacketTarget { get; set; }
        public int PacketSpeed { get; set; }
        
        public Guid? RouteTarget { get; set; }

        public Guid? StarbaseBattlePlanGuid { get; set; }

        public bool ContributesOnlyLeftoverToResearch { get; set; }
        public List<ProductionQueueItem> Items { get; set; } = new();

    }
}