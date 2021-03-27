using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Movement for a token
    /// </summary>
    public class BattleRecordTokenMove : BattleRecordTokenAction
    {
        /// <summary>
        /// The ending location of the token
        /// </summary>
        public Vector2 To { get; set; }

    }
}