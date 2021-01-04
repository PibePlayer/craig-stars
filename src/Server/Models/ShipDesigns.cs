using System;
using System.Collections.Generic;

namespace CraigStars
{
    public static class ShipDesigns
    {
        public static ShipDesign LongRangeScount = new ShipDesign()
        {
            Name = "Long Range Scout",
            Hull = Techs.Scout,
            HullSetNumber = 0,
            Slots = new List<ShipDesignSlot>()
            {
                new ShipDesignSlot(Techs.LongHump6, 0, 1),
                new ShipDesignSlot(Techs.FuelTank, 1, 1),
                new ShipDesignSlot(Techs.RhinoScanner, 2, 1),
            }
        };

    }
}