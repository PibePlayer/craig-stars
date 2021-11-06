using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token on a battle board
    /// </summary>
    public class BattleRecordToken
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        [DefaultValue(MapObject.Unowned)]
        public int PlayerNum { get; set; } = MapObject.Unowned;

        /// <summary>
        /// The token 
        /// </summary>
        public ShipToken Token { get; set; }

        /// <summary>
        /// The starting position of this token
        /// </summary>
        public Vector2 StartingPosition { get; set; }

        public override string ToString()
        {
            return $"Player {PlayerNum} {Token.Design.Name} ({Token.Quantity})";
        }

    }
}