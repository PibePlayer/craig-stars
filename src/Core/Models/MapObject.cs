using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public abstract class MapObject : SerializableMapObject, Discoverable
    {

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public String Name { get; set; } = "";

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

        public String RaceName { get; set; }
        public String RacePluralName { get; set; }

        /// <summary>
        /// For fleets we own, the Player field is populated
        /// Otherwise, the Owner field is populated
        /// </summary>
        [JsonProperty(IsReference = true)]
        public Player Player { get; set; }

        public MapObject()
        {
        }

        public bool OwnedBy(Player player)
        {
            return Player == player;
        }

    }
}