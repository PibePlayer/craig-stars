using Godot;
using System;
using System.Runtime.InteropServices;

namespace CraigStars
{
    public readonly struct DraggableTech
    {
        public readonly string name;
        public readonly int index;
        public readonly TechCategory category;
        public readonly HullSlotType hullSlotType;

        public DraggableTech(string name, TechCategory category, int index, HullSlotType hullSlotType = HullSlotType.None)
        {
            this.name = name;
            this.category = category;
            this.index = index;
            this.hullSlotType = hullSlotType;
        }

    }
}