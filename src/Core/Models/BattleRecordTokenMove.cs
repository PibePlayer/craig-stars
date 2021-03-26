using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Movement for a token
    /// </summary>
    public class BattleRecordTokenMove
    {
        /// <summary>
        /// The token moving
        /// </summary>
        public BattleToken Token { get; set; }

        /// <summary>
        /// The starting location of the token
        /// </summary>
        public Vector2 From { get; set; }

        /// <summary>
        /// The ending location of the token
        /// </summary>
        public Vector2 To { get; set; }

    }
}