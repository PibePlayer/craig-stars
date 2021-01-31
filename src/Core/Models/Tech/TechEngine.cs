using System.Collections.Generic;

namespace CraigStars
{
    public class TechEngine : TechHullComponent
    {
        private const int MaxWarp = 10;
        public int[] FuelUsage = new int[MaxWarp];

        public TechEngine()
        {
            Category = TechCategory.Engine;
            HullSlotType = HullSlotType.Engine;
        }

        public TechEngine(string name, Cost cost, TechRequirements techRequirements, int ranking) : base(name, cost, techRequirements, ranking, TechCategory.Engine)
        {
            HullSlotType = HullSlotType.Engine;
        }

    }
}
