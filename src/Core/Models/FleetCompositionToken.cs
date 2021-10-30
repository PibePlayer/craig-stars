using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A token in a FleetComposition
    /// </summary>
    public class FleetCompositionToken
    {
        /// <summary>
        /// What purpose this token serves
        /// </summary>
        public ShipDesignPurpose Purpose { get; set; }

        /// <summary>
        /// How many of this token we want
        /// </summary>
        public int Quantity { get; set; }

        public FleetCompositionToken() { }

        public FleetCompositionToken(ShipDesignPurpose purpose, int quantity)
        {
            Purpose = purpose;
            Quantity = quantity;
        }
    }
}