using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An order a player gives to some object like a fleet, ship design, etc.
    /// </summary>
    public abstract class PlayerObjectOrder
    {
        /// <summary>
        /// The Guid of the object so the server can track this on the server side
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Updated tags for this object
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();
    }
}