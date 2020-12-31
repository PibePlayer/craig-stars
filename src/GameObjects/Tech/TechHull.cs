using System.Collections.Generic;

namespace CraigStars
{
    public class TechHull : Tech
    {
        public int Mass { get; set; }
        public int Armor { get; set; }
        public int FuelCapacity { get; set; }
        public int FuelGenerationPerYear { get; set; }
        public int FleetHealBonus { get; set; }
        public bool DoubleMineEfficiency { get; set; }
        public bool BuiltInScannerForJoaT { get; set; }
        public bool Starbase { get; set; }
        public int Initiative { get; set; }
        public List<TechHullSlot> Slots { get; set; } = new List<TechHullSlot>();

        public TechHull()
        {
        }

        public TechHull(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

    }
}
