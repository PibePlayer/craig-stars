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
    public class BattleRecordToken
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public PublicPlayerInfo Owner
        {
            get
            {
                if (Player != null)
                {
                    owner = Player;
                }
                return owner;
            }
            set
            {
                owner = value;
            }
        }
        PublicPlayerInfo owner;

        [JsonIgnore]
        public Player Player { get; set; }

        /// <summary>
        /// The token 
        /// </summary>
        public ShipToken Token { get; set; }

        /// <summary>
        /// The starting position of this token
        /// </summary>
        public Vector2 StartingPosition { get; set; }

        /// <summary>
        /// This is used by the UI to store some state information
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public float DamageArmor { get; set; }

        /// <summary>
        /// This is used by the UI to store some state information
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public float DamageShield { get; set; }

        /// <summary>
        /// This is used by the UI to store some state information
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public float ShipsDestroyed { get; set; }

        public override string ToString()
        {
            return $"{Owner.RaceName} {Token.Design.Name} ({Token.Quantity})";
        }

    }
}