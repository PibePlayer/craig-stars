using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{

    public class GraphContent : ReferenceRect
    {
        static CSLog log = LogProvider.GetLogger(typeof(GraphContent));

        public Color GridColor { get; set; } = new Color("6e6e6e");
        public Vector2 AxisDivisions { get; set; } = new Vector2(1, 1);
        public List<Vector2> Points { get; set; }

        Line2D line;
        Label coordsLabel;

        public override void _Ready()
        {
            base._Ready();
            line = (Line2D)FindNode("Line2D");
            coordsLabel = (Label)FindNode("CoordsLabel");

            Connect("gui_input", this, nameof(OnGraphGuiInput));
        }

        void OnGraphGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                coordsLabel.Text = $"({mouseMotion.Position.x}, {RectSize.y - mouseMotion.Position.y})";
            }
        }

        public void UpdateLine()
        {
            float xScale = RectSize.x / AxisDivisions.x;
            float yScale = RectSize.y / AxisDivisions.y;
            log.Debug($"xScale: {xScale}, yScale: {yScale}");

            line.Points = Points.Select((point, index) => new Vector2(index * xScale, RectSize.y - point.y * yScale)).ToArray();
        }

        public override void _Draw()
        {
            base._Draw();

            UpdateLine();

            float xScale = RectSize.x / AxisDivisions.x;
            float yScale = RectSize.y / AxisDivisions.y;

            var scale = new Vector2(xScale, yScale);

            for (var y = 1; y < AxisDivisions.y; y++)
            {
                // draw horizontal lines up y axis
                DrawLine(new Vector2(0, y * yScale), new Vector2(RectSize.x, y * yScale), GridColor);
            }

            for (var x = 1; x < AxisDivisions.x; x++)
            {
                // draw vertical lines along the x axis
                DrawLine(new Vector2(x * xScale, 0), new Vector2(x * xScale, RectSize.y), GridColor);
            }

            Points.Each((point, index) => DrawCircle(new Vector2(index * scale.x, RectSize.y - point.y * scale.y), 5, line.DefaultColor));

        }

    }
}