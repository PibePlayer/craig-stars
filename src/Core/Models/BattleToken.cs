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
        /// A token can disengage after moving 7 times
        /// </summary>
        internal int MovesMade;

    }
}