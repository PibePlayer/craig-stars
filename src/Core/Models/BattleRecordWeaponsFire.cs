using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleRecordWeaponsFire : BattleRecordTokenAction
    {
        /// <summary>
        /// The slot with weapons that is firing
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// The target fired upon
        /// </summary>
        /// <value></value>
        public BattleRecordToken Target { get; set; }
    }
}