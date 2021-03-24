using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Aggregate of fleet data, compiled from ship design aggregates.
    /// </summary>
    public class FleetAggregate : ShipDesignAggregate
    {
        public HashSet<ShipDesignPurpose> Purposes = new HashSet<ShipDesignPurpose>();
    }
}