
namespace CraigStars
{
    public class ShipDesignSlot
    {
        public TechHullComponent HullComponent { get; set; } = new TechHullComponent();
        public int HullSlotIndex { get; set; } = 1;
        public int Quantity { get; set; }

        public ShipDesignSlot() { }

        public ShipDesignSlot(TechHullComponent hullComponent, int hullSlotIndex = 0, int quantity = 1)
        {
            HullComponent = hullComponent;
            HullSlotIndex = hullSlotIndex;
            Quantity = quantity;
        }
    }
}