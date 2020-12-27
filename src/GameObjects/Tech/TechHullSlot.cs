using System.Collections.Generic;

namespace CraigStars
{
    public class TechHullSlot
    {
        public HashSet<HullSlotType> Types { get; set; } = new HashSet<HullSlotType>();
        public int Capacity { get; set; }
        public bool Required { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
