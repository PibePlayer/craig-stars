using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// A token ran away
    /// </summary>
    public class BattleRecordTokenDestroyed : BattleRecordTokenAction
    {
        public BattleRecordTokenDestroyed(BattleRecordToken token) : base(token, Vector2.NegOne) { }
    }
}