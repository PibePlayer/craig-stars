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

        /// <summary>
        /// A stack of tokens needs to know how many of the tokens are damaged and 
        /// what percent that quanity is damaged
        /// </summary>
        /// <value></value>
        public int QuantityDamaged { get; set; }

        /// <summary>
        /// This is the percent the QuanityDamaged tokens are damaged
        /// </summary>
        /// <value></value>
        public int Damage { get; set; }

        public ShipToken() { }

        public ShipToken(ShipDesign design, int quantity)
        {
            Design = design;
            Quantity = quantity;
        }

    }
}