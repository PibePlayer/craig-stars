
using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Players submit a list of their ship designs and the server figures out if they should be added or deleted or updated
    /// </summary>
    public class ShipDesignOrder : PlayerObjectOrder
    {
        public ShipDesign Design { get; set; }
        public bool Deleted { get; set; }
        public bool New { get; set; }
    }
}