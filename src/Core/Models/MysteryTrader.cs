using System;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A mystery trader offering riches and delights!
    /// </summary>
    [JsonObject(IsReference = true)]
    public class MysteryTrader : MapObject
    {
        /// <summary>
        /// The amount of minerals requested
        /// </summary>
        public int AmountRequested { get; set; } = 5000;

        /// <summary>
        /// The warp factor this mystery trader is travelling at
        /// </summary>
        public int WarpFactor { get; set; }

        /// <summary>
        /// The heading this mystery trader is travelling along
        /// </summary>
        public Vector2 Heading { get; set; }
    }
}
