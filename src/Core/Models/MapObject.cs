using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using System.ComponentModel;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public abstract class MapObject : SerializableMapObject, IDiscoverable
    {
        /// <summary>
        /// Constant for this object being unexplored
        /// Used by Planets and MineFields
        /// </summary>
        public const int Unexplored = -1;
        public const int Unowned = -1;
        public const int Infinite = -1;

        public long Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public string Name { get; set; } = "";
        public string RaceName { get; set; }
        public string RacePluralName { get; set; }

        [DefaultValue(Unowned)]
        public int PlayerNum { get; set; } = Unowned;

        [JsonIgnore]
        public bool Owned { get => PlayerNum != Unowned; }

        /// <summary>
        /// Allow players and AIs to tag map objects 
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();

        public MapObject()
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Name}";
        }

        public bool OwnedBy(Player player)
        {
            return player != null && PlayerNum == player.Num;
        }

        public bool OwnedBy(int playerNum)
        {
            return PlayerNum == playerNum;
        }

    }
}