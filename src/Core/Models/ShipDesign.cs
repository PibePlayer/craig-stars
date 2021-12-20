using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class ShipDesign : IDiscoverable
    {
        public enum DesignStatus { Current, New, Deleted }

        static CSLog log = LogProvider.GetLogger(typeof(ShipDesign));
        public string Name { get; set; }
        public int Version { get; set; } = 1;
        public Guid Guid { get; set; } = Guid.NewGuid();
        public DesignStatus Status { get; set; }

        [DefaultValue(MapObject.Unowned)]
        public int PlayerNum { get; set; } = MapObject.Unowned;

        public ShipDesignPurpose Purpose { get; set; }
        public TechHull Hull { get; set; } = new TechHull();
        public int HullSetNumber { get; set; }
        [DefaultValue(true)]
        public bool CanDelete { get; set; } = true;
        public List<ShipDesignSlot> Slots { get; set; } = new List<ShipDesignSlot>();

        [JsonIgnore]
        public bool InUse { get => NumInUse > 0; }
        [JsonIgnore]
        public bool Deleted { get => Status == ShipDesign.DesignStatus.Deleted; }
        public int NumInUse { get; set; }
        public int NumBuilt { get; set; }

        /// <summary>
        /// An spec of all components of a ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesignSpec Spec { get; set; } = new ShipDesignSpec();

        // public spec values
        public int Shields { get => Spec.Shield; set => Spec.Shield = value; }
        public int Armor { get => Spec.Armor; set => Spec.Armor = value; }

        public override string ToString()
        {
            return $"Player {PlayerNum} {Name}";
        }

        /// <summary>
        /// Create a clone of this ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesign Clone(Player player = null)
        {
            var design = Copy();
            design.Name = Name;
            design.Guid = Guid;
            design.PlayerNum = player != null ? player.Num : PlayerNum;
            return design;
        }

        /// <summary>
        /// Create a copy of this ship design with no name
        /// </summary>
        /// <returns></returns>
        public ShipDesign Copy()
        {
            var clone = new ShipDesign()
            {
                PlayerNum = PlayerNum,
                Hull = Hull,
                HullSetNumber = HullSetNumber,
                Purpose = Purpose,
                Version = Version,
                Slots = Slots.Select(s => new ShipDesignSlot(s.HullComponent, s.HullSlotIndex, s.Quantity)).ToList()
            };
            return clone;
        }

        /// <summary>
        /// Return true if this is a valid design.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var requiredHullSlotByIndex = new Dictionary<int, TechHullSlot>();
            for (int i = 0; i < Hull.Slots.Count; i++)
            {
                if (Hull.Slots[i].Required)
                {
                    requiredHullSlotByIndex[i + 1] = Hull.Slots[i];
                }
            }
            int filledRequiredSlots = 0;
            foreach (var slot in Slots)
            {
                if (requiredHullSlotByIndex.TryGetValue(slot.HullSlotIndex, out var hullSlot))
                {
                    if (slot.Quantity == hullSlot.Capacity)
                    {
                        filledRequiredSlots++;
                    }
                }
            }

            // return true if we have filled all required slots
            return filledRequiredSlots == requiredHullSlotByIndex.Count;
        }

    }
}