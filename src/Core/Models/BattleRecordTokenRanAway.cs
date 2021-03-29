using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// A token ran away
    /// </summary>
    public class BattleRecordTokenRanAway : BattleRecordTokenAction
    {
        public BattleRecordTokenRanAway()
        {
        }

        public BattleRecordTokenRanAway(BattleRecordToken token) : base(token, Vector2.NegOne) { }

        public override string ToString()
        {
            return $"{Token} ran away";
        }

    }
}