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
        /// <summary>
        /// Constant for this object being unexplored
        /// Used by Planets and MineFields
        /// </summary>
        public const int Unexplored = -1;

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public string Name { get; set; } = "";

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

        public string RaceName
        {
            get
            {
                if (raceName == null && Player != null)
                {
                    raceName = Player.Race.Name;
                }
                return raceName;
            }
            set => raceName = value;
        }
        string raceName;

        public string RacePluralName
        {
            get
            {
                if (racePluralName == null && Player != null)
                {
                    racePluralName = Player.Race.PluralName;
                }
                return racePluralName;
            }
            set => racePluralName = value;
        }
        string racePluralName;

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