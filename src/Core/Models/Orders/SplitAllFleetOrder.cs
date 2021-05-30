using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An immediate (client side) order to split all of a fleet's tokens into new fleets
    /// </summary>
    [JsonObject(IsReference = true)]
    public class SplitAllFleetOrder : FleetOrder
    {
        public List<Guid> NewFleetGuids { get; set; } = new List<Guid>();
    }
}