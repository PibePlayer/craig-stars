using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An immediate merge order (i.e. the client clicks a merge in the UI) merging one or more fleets
    /// into a source fleet
    /// </summary>
    [JsonObject(IsReference = true)]
    public class MergeFleetOrder : ImmediateFleetOrder
    {
        public List<Guid> MergingFleetGuids { get; set; } = new();

        public MergeFleetOrder() { }

        public MergeFleetOrder(Guid guid, params Fleet[] fleets)
        {
            Guid = guid;
            MergingFleetGuids.AddRange(fleets.Select(fleet => fleet.Guid));
        }

    }
}