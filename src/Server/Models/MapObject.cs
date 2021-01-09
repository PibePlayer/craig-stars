using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;
using System.Text.Json.Serialization;

namespace CraigStars
{
    public abstract class MapObject : SerializableMapObject
    {

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public String Name { get; set; } = "";

        [JsonIgnore]
        public Player Player { get; set; }
        public String RaceName { get; set; }
        public String RacePluralName { get; set; }

        /// <summary>
        /// We use the Player's number to load players after serializing
        /// </summary>
        /// <value></value>
        public int PlayerNum
        {
            get
            {
                if (Player != null && playerNum == -1)
                {
                    playerNum = Player.Num;
                }
                return playerNum;
            }
            set
            {
                playerNum = value;
            }
        }
        int playerNum = -1;

        public bool OwnedBy(Player player)
        {
            return Player == player;
        }

        #region Serializer Helpers

        /// <summary>
        /// Prepare this object for serialization
        /// </summary>
        public virtual void PreSerialize()
        {
            // before serialization, null out our player number
            playerNum = -1;
        }

        public virtual void PostSerialize(Dictionary<Guid, MapObject> mapObjectsByGuid) { }

        /// <summary>
        /// Wire up player objects to match the list of current players
        /// </summary>
        /// <param name="players"></param>
        public virtual void WireupPlayer(List<Player> players)
        {
            if (playerNum >= 0 && playerNum < players.Count)
            {
                Player = players[playerNum];
            }
        }

        #endregion
    }
}