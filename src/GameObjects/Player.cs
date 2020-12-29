using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class Player : Node
    {
        public int NetworkId { get; set; }
        public int Num { get; set; }
        public string PlayerName { get; set; }
        public Boolean Ready { get; set; } = false;
        public Boolean AIControlled { get; set; }
        public Color Color { get; set; } = Colors.Black;
        public Race Race = new Race();
        public Planet Homeworld { get; set; }

        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public List<Fleet> AlienFleets { get; set; } = new List<Fleet>();

        public override void _Ready()
        {

        }

    }
}
