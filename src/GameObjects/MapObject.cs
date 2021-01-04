using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public abstract class MapObject
    {

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2 Position { get; set; }
        public String ObjectName { get; set; } = "";
        public Player Player { get; set; }
        public String RaceName { get; set; }
        public String RacePluralName { get; set; }

        public bool OwnedBy(Player player)
        {
            return Player == player;
        }
    }
}