using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Action for a token
    /// </summary>
    public abstract class BattleRecordTokenAction
    {
        /// <summary>
        /// The token taking the action
        /// </summary>
        public BattleRecordToken Token { get; set; }

        /// <summary>
        /// The location of the token
        /// </summary>
        public Vector2 From { get; set; }

    }
}