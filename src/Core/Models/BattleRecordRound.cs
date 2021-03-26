using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A single round in a battle. 
    /// </summary>
    public class BattleRecordRound
    {
        public int RoundNumber { get; set; }

        /// <summary>
        /// The state of the board (after movement) for this round
        /// </summary>
        public List<BattleToken>[,] Grid = new List<BattleToken>[10, 10];

        public List<BattleRecordTokenMove> Movements = new List<BattleRecordTokenMove>();

        public List<BattleRecordWeaponsFire> WeaponsFirings = new List<BattleRecordWeaponsFire>();
    }
}