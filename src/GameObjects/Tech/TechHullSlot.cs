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

        public TechHullSlot() { }

        public TechHullSlot(HullSlotType[] types, int capacity = 0, bool required = false, int x = 0, int y = 0, int width = 0, int height = 0)
        {
            Types = new HashSet<HullSlotType>(types);
            Capacity = capacity;
            Required = required;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

    }
}
