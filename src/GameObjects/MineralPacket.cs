using Godot;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Singletons;
using System;

namespace CraigStars
{
    /// <summary>
    /// A mineral packet flying through space
    /// </summary>
    public class MineralPacket : MapObject
    {
        public Waypoint Target { get; set; } = new Waypoint();
        public Mineral Contents { get; set; }

    }
}
