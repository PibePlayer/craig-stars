using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    public abstract class MapObject : SerializableMapObject
    {

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public String Name { get; set; } = "";

        public Player Player { get; set; }
        public String RaceName { get; set; }
        public String RacePluralName { get; set; }

        public MapObject()
        {
        }

        public bool OwnedBy(Player player)
        {
            return Player == player;
        }

    }
}