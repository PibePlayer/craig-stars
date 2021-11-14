using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CraigStars
{
    public class TechHull : Tech
    {
        public const int UnlimitedSpaceDock = int.MaxValue;
        public TechHullType Type { get; set; }
        public int Mass { get; set; }
        public int Armor { get; set; }
        public int FuelCapacity { get; set; }
        public int FuelGenerationPerYear { get; set; }
        public int FleetHealBonus { get; set; }
        public bool DoubleMineEfficiency { get; set; }
        public bool BuiltInScanner { get; set; }
        public int CargoCapacity { get; set; }

        /// <summary>
        /// Starbases have +1 range
        /// </summary>
        public int RangeBonus { get; set; }
        public bool Starbase { get; set; }
        [DefaultValue(UnlimitedSpaceDock)]
        public int SpaceDock { get; set; } = UnlimitedSpaceDock;
        public int Initiative { get; set; }
        public List<TechHullSlot> Slots { get; set; } = new List<TechHullSlot>();

        public TechHull()
        {
        }

        public TechHull(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

        /// <summary>
        /// Return true if this hull can use a certain component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool CanUse(TechHullComponent component)
        {
            return Slots.Find(slot => slot.Type.HasFlag(component.HullSlotType)) != null;
        }

    }
}
