using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleRecordWeaponsFire
    {
        /// <summary>
        /// The token firing
        /// </summary>
        public BattleToken Token { get; set; }

        /// <summary>
        /// The slot with weapons that is firing
        /// </summary>
        public int Slot { get; set; }
    }
}