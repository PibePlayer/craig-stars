using System.Collections.Generic;

namespace CraigStars
{
    public class ShipDesign
    {
        public string Name { get; set; }
        public TechHull Hull { get; set; } = new TechHull();
        public int HullSetNumber { get; set; }
        public List<ShipDesignSlot> Slots { get; set; } = new List<ShipDesignSlot>();

        /// <summary>
        /// An aggregate of all components of a ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesignAggregate Aggregate { get; } = new ShipDesignAggregate();
    }
}