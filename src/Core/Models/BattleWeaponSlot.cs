using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleWeaponSlot
    {
        /// <summary>
        /// The token with the weapon
        /// </summary>
        public BattleToken Token { get; set; }

        /// <summary>
        /// The weapon slot
        /// </summary>
        public ShipDesignSlot Slot { get; set; }

        public BattleWeaponSlot(BattleToken token, ShipDesignSlot slot)
        {
            Token = token;
            Slot = slot;
        }

        /// <summary>
        /// Return true if this weapon slot is in range of the token target
        /// </summary>
        /// <returns></returns>
        public bool IsInRange()
        {
            if (Token.Target == null)
            {
                return false;
            }
            var distance = Math.Abs(Token.Position.x - Token.Target.Position.x) + Math.Abs(Token.Position.y - Token.Target.Position.y);
            return distance <= Slot.HullComponent.Range;
        }

    }
}