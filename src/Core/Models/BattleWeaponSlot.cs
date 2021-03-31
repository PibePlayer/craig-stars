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
        /// The type of weapon this weapon slot is
        /// </summary>
        /// <value></value>
        public BattleWeaponType WeaponType { get; set; }

        /// <summary>
        /// Each weapon has a potential list of targets that is updated each turn
        /// This list is sorted by attractiveness
        /// </summary>
        /// <typeparam name="BattleToken"></typeparam>
        /// <returns></returns>
        public List<BattleToken> Targets { get; set; } = new List<BattleToken>();

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
            if (slot.HullComponent.Category == TechCategory.BeamWeapon)
            {
                WeaponType = BattleWeaponType.Beam;
            }
            else if (slot.HullComponent.Category == TechCategory.Torpedo)
            {
                WeaponType = BattleWeaponType.Torpedo;
            }
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