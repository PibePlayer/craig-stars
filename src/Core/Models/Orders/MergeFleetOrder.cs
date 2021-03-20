using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// An immediate merge order (i.e. the client clicks a merge in the UI) merging one or more fleets
    /// into a source fleet
    /// </summary>
    public class MergeFleetOrder : FleetOrder
    {
        public List<Fleet> MergingFleets { get; set; }
    }
}