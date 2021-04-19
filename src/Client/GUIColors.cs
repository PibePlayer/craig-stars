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


        [Export]
        public Color IroniumBarColor { get; set; } = new Color("0900FF");

        [Export]
        public Color BoraniumBarColor { get; set; } = new Color("00FF00");

        [Export]
        public Color GermaniumBarColor { get; set; } = new Color("FFFF00");

        [Export]
        public Color IroniumConcentrationColor { get; set; } = new Color("020085");

        [Export]
        public Color BoraniumConcentrationColor { get; set; } = new Color("008100");

        [Export]
        public Color GermaniumConcentrationColor { get; set; } = new Color("7F7F00");

        [Export]
        public Color OwnedMineFieldColor { get; set; } = new Color("0900FF");

        [Export]
        public Color WaypointLineColor { get; set; } = new Color("0900FF");

        [Export]
        public Color CommandedWaypointLineColor { get; set; } = new Color("0900FF").Lightened(.2f);

        [Export]
        public Color HabitableColor { get; set; } = new Color("00FF00");

        [Export]
        public Color HabitableOutlineColor { get; set; } = new Color("008100");


        [Export]
        public Color UninhabitableColor { get; set; } = new Color("FF0000");

        [Export]
        public Color UninhabitableOutlineColor { get; set; } = new Color("810000");

        [Export]
        public Color TerraformableColor { get; set; } = new Color("00FF00");

        [Export]
        public Color TerraformableOutlineColor { get; set; } = new Color("008100");

        [Export]
        public Color OwnedColor { get; set; } = new Color("00FF00");

        [Export]
        public Color FriendColor { get; set; } = new Color("FFFF00");

        [Export]
        public Color EnemyColor { get; set; } = new Color("FF0000");

        [Export]
        public Color FriendAndEnemyColor { get; set; } = new Color("6A0DAD");

        [Export]
        public Color ScannerColor { get; set; } = new Color("8B0000");

        [Export]
        public Color ScannerPenColor { get; set; } = new Color("7F7F00");


        [Export]
        public Color FuelColor { get; set; } = new Color("FF0000");

        [Export]
        public Color WarpColor { get; set; } = new Color("FF0000");

        [Export]
        public Color WarpDamageColor { get; set; } = new Color("FFFF00");

        [Export]
        public Color StargateColor { get; set; } = new Color("FFFF00");

    }
}