using System.Text.Json.Serialization;

namespace CraigStars
{
    public class ShipDesignSlot
    {
        [JsonIgnore]
        public TechHullComponent HullComponent { get; set; } = new TechHullComponent();
        public int HullSlotIndex { get; set; } = 1;
        public int Quantity { get; set; }

        #region Serializer Helpers

        public string HullComponentName
        {
            get
            {
                if (hullComponentName == null)
                {
                    hullComponentName = HullComponent.Name;
                }
                return hullComponentName;
            }
            set
            {
                hullComponentName = value;
            }
        }
        string hullComponentName;

        /// <summary>
        /// Prepare this object for serialization
        /// </summary>
        public void PreSerialize()
        {
            HullComponentName = null;
        }

        /// <summary>
        /// After serialization, wire up values we stored by key
        /// </summary>
        /// <param name="techStore"></param>
        public void PostSerialize(ITechStore techStore)
        {
            HullComponent = techStore.GetTechByName<TechHullComponent>(HullComponentName);
        }

        #endregion

        public ShipDesignSlot() { }

        public ShipDesignSlot(TechHullComponent hullComponent, int hullSlotIndex = 0, int quantity = 1)
        {
            HullComponent = hullComponent;
            HullSlotIndex = hullSlotIndex;
            Quantity = quantity;
        }
    }
}