using Godot;
using System;

namespace CraigStars
{
    public class MineFieldSprite : MapObjectSprite
    {
        [Export]
        int CirclePoints
        {
            get => circlePoints;
            set
            {
                circlePoints = value;
                UpdatePolygon();
            }
        }
        int circlePoints = 64;

        [Export]
        float Radius
        {
            get => radius;
            set
            {
                radius = value;
                UpdatePolygon();
            }
        }
        float radius = 32;

        /// <summary>
        /// Convenience method so the code looks like MineField.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public MineField MineField
        {
            get => MapObject as MineField;
            set
            {
                MapObject = value;
            }
        }

        Polygon2D polygon;

        public override void _Ready()
        {
            polygon = GetNode<Polygon2D>("Polygon2D");
            UpdatePolygon();
        }

        void UpdatePolygon()
        {
            if (polygon == null)
            {
                return;
            }
            var circlePointStep = Mathf.Tau / CirclePoints;
            var points = new Vector2[CirclePoints];
            var colors = new Color[CirclePoints];
            var uv = new Vector2[(CirclePoints - 2) * 2];

            for (int i = 0; i < CirclePoints; i++)
            {
                float angle = i * circlePointStep;
                points[i] = new Vector2(
                    (float)(Math.Cos(angle) * Radius),
                    (float)(Math.Sin(angle) * Radius)
                );
                colors[i] = Colors.White;
            }
            polygon.Polygon = points;
            polygon.VertexColors = colors;
        }

        public override void UpdateSprite()
        {
            // do nothing (maybe we will change color on select)
        }
    }
}