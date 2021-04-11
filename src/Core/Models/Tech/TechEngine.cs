using System.Collections.Generic;

namespace CraigStars
{
    public class TechEngine : TechHullComponent
    {
        private const int MaxWarp = 10;

        /// <summary>
        /// This is both the default speed for WarpFactor waypoints and used
        /// to determine the number of moves in battle
        /// </summary>
        public int IdealSpeed;

        /// <summary>
        /// The speed at which this engine consumes no fuel
        /// </summary>
        public int FreeSpeed = 1;
        
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
