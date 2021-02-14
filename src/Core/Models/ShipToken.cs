using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A stack of a single ShipDesign type in a fleet.  A fleet is made up of many Ship Stacks
    /// </summary>
    public class ShipToken
    {
        [JsonProperty(IsReference = true)]
        public ShipDesign Design { get; set; } = new ShipDesign();
        public int Quantity { get; set; }

        public ShipToken() { }

        public ShipToken(ShipDesign design, int quantity)
        {
            Design = design;
            Quantity = quantity;
        }

    }
}