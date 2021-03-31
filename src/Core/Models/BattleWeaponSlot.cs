using System;
using System.Collections.Generic;
using Godot;

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

        /// <summary>
        /// The range of this weapon
        /// </summary>
        /// <value></value>
        public int Range
        {
            get
            {
                if (Slot != null)
                {
                    return Slot.HullComponent.Range;
                }
                return 0;
            }
        }

        public BattleWeaponSlot(BattleToken token, ShipDesignSlot slot)
        {
            Token = token;
            Slot = slot;
        }

        /// <summary>
        /// True if this weapon is in slow of it's target
        /// </summary>
        /// <returns></returns>
        public bool IsInRange()
        {
            return IsInRange(Token.Target) || IsInRange(Token.TargetOfOpportunity);
        }

        /// <summary>
        /// Return true if this weapon slot is in range of the token target
        /// </summary>
        /// <returns></returns>
        public bool IsInRange(BattleToken token)
        {
            if (token == null)
            {
                return false;
            }
            return IsInRange(token.Position);
        }

        public bool IsInRange(Vector2 position)
        {
            // diagonal shots count as one move, so we take the max distance on the x or y as our actual distance away
            // i.e. 4 over, 1 up is 4 range away, 3 over 2 up is 3 range away, etc.
            return Token.GetDistanceAway(position) <= Range;
        }

        public bool IsInRange(int range)
        {
            return range <= Range;
        }
    }
}