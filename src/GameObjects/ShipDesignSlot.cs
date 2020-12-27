namespace CraigStars
{
    public class ShipDesignSlot
    {
        public TechHullSlot HullSlot { get; set; } = new TechHullSlot();
        public TechHullComponent HullComponent { get; set; } = new TechHullComponent();
        public int Quantity { get; set; }
    }
}