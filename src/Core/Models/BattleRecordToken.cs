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
    }
}