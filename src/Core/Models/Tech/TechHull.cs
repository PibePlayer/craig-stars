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

        /// <summary>
        /// True if this hull is a starbase
        /// </summary>
        /// <value></value>
        public bool Starbase { get; set; }

        /// <summary>
        /// The spacedoc determines what size ships this starbase can build
        /// </summary>
        /// <value></value>
        [DefaultValue(UnlimitedSpaceDock)]
        public int SpaceDock { get; set; } = UnlimitedSpaceDock;

        /// <summary>
        /// The factor this Starbase will be modify the innate ScanRange for pen scanning
        /// For AR races, the Ultra Station and Death Star have half the innate scan range as pen scanning
        /// </summary>
        public float InnateScanRangePenFactor;

        /// <summary>
        /// If true, this hull is built by orbital construction modules
        /// </summary>
        /// <value></value>
        public bool OrbitalConstructionHull { get; set; }

        public int Initiative { get; set; }
        public float RepairBonus { get; set; }

        [DefaultValue(1f)]
        public float MineLayingFactor { get; set; } = 1f;
        public bool ImmuneToOwnDetonation { get; set; }

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
