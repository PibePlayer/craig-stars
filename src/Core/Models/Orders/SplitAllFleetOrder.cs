using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// An immediate (client side) order to split all of a fleet's tokens into new fleets
    /// </summary>
    public class SplitAllFleetOrder : FleetOrder
    {
        public List<Guid> NewFleetGuids { get; set; } = new List<Guid>();
    }
}