using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    public class Universe : Node2D
    {
        public List<Planet> Planets { get; set; } = new List<Planet>();

        public int Width { get; set; }
        public int Height { get; set; }
    }
}