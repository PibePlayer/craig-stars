using Godot;

namespace CraigStars
{
    public class GUIColors : Resource
    {
        [Export]
        public Color GravColor { get; set; } = new Color("020085");

        [Export]
        public Color TempColor { get; set; } = new Color("8B0000");

        [Export]
        public Color RadColor { get; set; } = new Color("008100");

        [Export]
        public Color GravValueColor { get; set; } = new Color("0900FF");

        [Export]
        public Color TempValueColor { get; set; } = new Color("FF0000");

        [Export]
        public Color RadValueColor { get; set; } = new Color("00FF00");

        [Export]
        public Color HabitablePlanetTextColor { get; set; } = new Color("008100");

        [Export]
        public Color UninhabitablePlanetTextColor { get; set; } = new Color("8B0000");

    }
}