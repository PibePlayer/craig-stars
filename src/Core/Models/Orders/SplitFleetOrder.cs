using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// An immediate (client side) order to split a fleet into new fleets
    /// </summary>
    public class SplitFleetOrder : FleetOrder
    {
        /// <summary>
        /// The new fleets that were created from the tokens of an existing fleet
        /// </summary>
        /// <value></value>
        public List<Fleet> SplitFleets { get; set; }
    }
}