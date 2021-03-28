using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token on a battle board
    /// </summary>
    public class BattleToken : BattleRecordToken
    {
        /// <summary>
        /// This will be null during the recording, but populated 
        /// during battle generation
        /// </summary>
        internal Fleet Fleet { get; set; }

        /// <summary>
        /// The type of target this token is
        /// </summary>
        internal BattleTokenAttribute Attributes { get; set; }

        /// <summary>
        /// This token's target
        /// </summary>
        internal BattleToken Target { get; set; }

        /// <summary>
        /// Tokens targeting this token
        /// </summary>
        internal List<BattleToken> TargetedBy { get; set; } = new List<BattleToken>();

        /// <summary>
        /// The token's current position on the board
        /// </summary>
        /// <value></value>
        internal Vector2 Position { get; set; }

        /// <summary>
        /// Has this token been destroyed? (it won't move anymore)
        /// </summary>
        internal bool Destroyed;

        /// <summary>
        /// Has this token been damaged? 
        /// </summary>
        internal bool Damaged;

        /// <summary>
        /// Has this token successfully run away? 
        /// </summary>
        internal bool RanAway;

        /// <summary>
        /// A token can disengage after moving 7 times
        /// </summary>
        internal int MovesMade;

        /// <summary>
        /// The remaining shields for this stack, computed at the beginning of battle
        /// </summary>
        internal int Shields;

        public override string ToString()
        {
            return $"{Fleet.RaceName} {Token.Design.Name} ({Token.Quantity})";
        }

        /// <summary>
        /// Return this token's distance from another position on the board.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        internal int GetDistanceAway(Vector2 position)
        {
            return (int)Math.Max(Math.Abs(Position.x - position.x), Math.Abs(Position.y - position.y));
        }

        /// <summary>
        /// Get the distance we are from this weapon
        /// </summary>
        /// <param name="weaponSlot"></param>
        /// <returns></returns>
        internal int GetDistanceAway(BattleWeaponSlot weaponSlot)
        {
            return GetDistanceAway(weaponSlot.Token.Position);
        }

    }
}