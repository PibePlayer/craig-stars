using System.Collections.Generic;

namespace CraigStars
{
    public class TechHullSlot
    {
        public HullSlotType Type { get; set; }
        public int Capacity { get; set; }
        public bool Required { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TechHullSlot() { }

        public TechHullSlot(HullSlotType type, int capacity = 0, bool required = false, int x = 0, int y = 0, int width = 0, int height = 0)
        {
            Type = type;
            Capacity = capacity;
            Required = required;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

    }
}
