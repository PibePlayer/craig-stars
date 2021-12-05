using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    [Tool]
    public class MineralBar : Control
    {
        [Export]
        public MineralType Type
        {
            get => type;
            set
            {
                type = value;
                Update();
            }
        }
        MineralType type = MineralType.Ironium;

        [Export]
        public int Concentration
        {
            get => concentration;
            set
            {
                concentration = value;
                Update();
            }
        }
        int concentration = 50;

        [Export]
        public int Surface
        {
            get => surface;
            set
            {
                surface = value;
                Update();
            }
        }
        int surface = 500;

        [Export]
        public int MiningRate
        {
            get => miningRate;
            set
            {
                miningRate = value;
                Update();
            }
        }
        int miningRate = 50;

        /// <summary>
        /// The scale of this graph, i.e. how many surface minerals
        /// represent 100% of the bar being filled
        /// </summary>
        /// <value></value>
        [Export]
        public int Scale
        {
            get => scale;
            set
            {
                scale = value;
                Update();
            }
        }
        int scale = 5000;

        public override void _Ready()
        {
            Update();
        }

        public override void _Draw()
        {
            Color concentrationColor = Colors.White;
            Color miningRateColor = Colors.White;
            Color barColor = Colors.White;
            switch (Type)
            {
                case MineralType.Ironium:
                    concentrationColor = miningRateColor = GUIColorsProvider.Colors.IroniumConcentrationColor;
                    barColor = GUIColorsProvider.Colors.IroniumBarColor;
                    break;
                case MineralType.Boranium:
                    concentrationColor = miningRateColor = GUIColorsProvider.Colors.BoraniumConcentrationColor;
                    barColor = GUIColorsProvider.Colors.BoraniumBarColor;
                    break;
                case MineralType.Germanium:
                    concentrationColor = miningRateColor = GUIColorsProvider.Colors.GermaniumConcentrationColor;
                    barColor = GUIColorsProvider.Colors.GermaniumBarColor;
                    break;
            }

            // draw a bar representing surface minerals
            DrawRect(new Rect2(Vector2.Zero, RectSize), Colors.Black, true);

            float surfacePercent = Mathf.Clamp((float)Surface / Scale, 0, 1);
            Vector2 surfaceBarEnd = new Vector2(RectSize.x * surfacePercent, RectSize.y);
            DrawRect(new Rect2(
                Vector2.Zero,
                surfaceBarEnd
            ), barColor);

            // add the mining rate bar
            // make sure it doesn't go over
            float miningRatePercent = Mathf.Clamp((float)MiningRate / Scale, 0, 1);
            DrawRect(new Rect2(
                new Vector2(surfaceBarEnd.x, 0),
                new Vector2(Mathf.Clamp(RectSize.x * miningRatePercent, 0, RectSize.x - surfaceBarEnd.x), RectSize.y)
            ), miningRateColor);

            // draw a diamond representing the concentration
            float concentrationPercent = Mathf.Clamp(Concentration / 100.0f, 0, 1);
            Vector2 concentrationPosition = new Vector2((RectSize.x * concentrationPercent), RectPosition.y + RectSize.y / 2);
            this.DrawDiamond(concentrationPosition, RectSize.y, concentrationColor);
        }
    }
}