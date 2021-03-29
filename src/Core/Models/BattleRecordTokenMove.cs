using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Movement for a token
    /// </summary>
    public class BattleRecordTokenMove : BattleRecordTokenAction
    {
        public BattleRecordTokenMove()
        {
        }

        public BattleRecordTokenMove(BattleRecordToken token, Vector2 from, Vector2 to) : base(token, from)
        {
            To = to;
        }

        /// <summary>
        /// The ending location of the token
        /// </summary>
        public Vector2 To { get; set; }

        public override string ToString()
        {
            return $"{Token} moved from {From} to {To}";
        }
    }
}