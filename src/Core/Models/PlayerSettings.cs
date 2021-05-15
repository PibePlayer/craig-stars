using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// This stores settings the player has configured for this game, like
    /// which turn processors to use
    /// </summary>
    public class PlayerSettings
    {
        /// <summary>
        /// The turn processors that are enabled for this player
        /// </summary>
        public List<string> TurnProcessors { get; set; } = new List<string>();

    }
}