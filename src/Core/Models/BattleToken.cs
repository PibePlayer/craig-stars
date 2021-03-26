using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token on a battle board
    /// </summary>
    public class BattleToken
    {
        public Player Player { get; set; }

        /// <summary>
        /// The token 
        /// </summary>
        public ShipToken Token { get; set; }

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
        /// The token's current position on the board
        /// </summary>
        /// <value></value>
        internal Vector2 Position { get; set; }

    }
}