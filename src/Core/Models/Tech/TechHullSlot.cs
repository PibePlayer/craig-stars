using System.Collections.Generic;

namespace CraigStars
{
    public class TechHullSlot
    {
        public HullSlotType Type { get; set; }
        public int Capacity { get; set; }
        public bool Required { get; set; }

        public TechHullSlot() { }

        public TechHullSlot(HullSlotType type, int capacity = 1, bool required = false)
        {
            Type = type;
            Capacity = capacity;
            Required = required;
        }

    }
}
