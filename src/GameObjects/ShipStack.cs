using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A stack of a single ShipDesign type in a fleet.  A fleet is made up of many Ship Stacks
    /// </summary>
    public class ShipStack
    {
        public ShipDesign Design { get; set; } = new ShipDesign();
        public int Quantity { get; set; }
    }
}