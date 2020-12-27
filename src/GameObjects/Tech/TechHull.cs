using System.Collections.Generic;

namespace CraigStars
{
    public class TechHull : Tech
    {
        public int mass { get; set; }
        public int armor { get; set; }
        public int fuelCapacity { get; set; }
        public int fuelGenerationPerYear { get; set; }
        public int fleetHealBonus { get; set; }
        public bool doubleMineEfficiency { get; set; }
        public bool builtInScannerForJoaT { get; set; }
        public bool starbase { get; set; }
        public int initiative { get; set; }
        public List<TechHullSlot> slots { get; set; } = new List<TechHullSlot>();
    }
}
