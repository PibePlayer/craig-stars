using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class PlanetSpriteMinerals : Control
    {
        public Mineral Mineral { get; set; } = new Mineral(2500, 1000, 800);

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

        public override void _Draw()
        {
            base._Draw();

            if (Mineral == Mineral.Empty)
            {
                // only show mineral views we can see
                return;
            }

            // draw the left side and bottom bars
            DrawRect(new Rect2(new Vector2(), new Vector2(1, RectSize.y)), Colors.Gray);
            DrawRect(new Rect2(new Vector2(0, RectSize.y), new Vector2(RectSize.x, 1)), Colors.Gray);

            int xOffset = 2; // move each bar 1 pixel over from the line
            int yOffset = 0;
            int barWidth = 9;
            int barOffset = 1;

            // draw mineral bars
            float percent = Mathf.Clamp((float)Mineral.Ironium / Scale, 0, 1);
            DrawRect(
                new Rect2(
                    new Vector2(xOffset, RectSize.y - yOffset),
                    barWidth, -(RectSize.y - yOffset) * percent
                ),
                GUIColorsProvider.Colors.IroniumBarColor
            );

            percent = Mathf.Clamp((float)Mineral.Boranium / Scale, 0, 1);
            DrawRect(
                new Rect2(
                    new Vector2(xOffset + barWidth + barOffset, RectSize.y - yOffset),
                    barWidth, -(RectSize.y - yOffset) * percent
                ),
                GUIColorsProvider.Colors.BoraniumBarColor
            );

            percent = Mathf.Clamp((float)Mineral.Germanium / Scale, 0, 1);
            DrawRect(
                new Rect2(
                    new Vector2(xOffset + barWidth * 2 + barOffset * 2, RectSize.y - yOffset),
                    barWidth, -(RectSize.y - yOffset) * percent
                ),
                GUIColorsProvider.Colors.GermaniumBarColor
            );

        }
    }
}