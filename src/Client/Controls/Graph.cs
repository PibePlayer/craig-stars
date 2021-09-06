using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    /// <summary>
    /// A graph of engine fuel usage by warp
    /// Y axis is fuel usage from 0 to 1000%
    /// X axis is warp from 0 to 10
    /// </summary>
    [Tool]
    public class Graph : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(Graph));

        public record AxisLabel(string label, float value, Color color, bool italic = false) { };

        [Export]
        public string Title
        {
            get => title;
            set
            {
                title = value;
                if (titleLabel != null)
                {
                    titleLabel.Text = title;
                }
            }
        }
        string title;

        [Export]
        public string XAxisTitle
        {
            get => xAxisTitle;
            set
            {
                xAxisTitle = value;
                if (xAxisLabel != null)
                {
                    xAxisLabel.Text = xAxisTitle;
                }
            }
        }
        string xAxisTitle;

        public virtual List<AxisLabel> XAxisLabels { get; set; } = new();
        public virtual List<AxisLabel> YAxisLabels { get; set; } = new();

        Label titleLabel;
        Label xAxisLabel;
        protected Container xAxis;
        protected Container yAxis;
        GraphContent graphContent;

        int yLabels = 0;

        public override void _Ready()
        {
            xAxis = GetNode<Container>("VBoxContainer/GraphContainer/XAxis");
            yAxis = GetNode<Container>("VBoxContainer/GraphContainer/YAxis");
            graphContent = GetNode<GraphContent>("VBoxContainer/GraphContainer/GraphContent");
            titleLabel = GetNode<Label>("VBoxContainer/Title");
            xAxisLabel = GetNode<Label>("VBoxContainer/GraphContainer/XAxisLabel");

            foreach (Node node in xAxis.GetChildren())
            {
                xAxis.RemoveChild(node);
                node.QueueFree();
            }
            foreach (Node node in yAxis.GetChildren())
            {
                yAxis.RemoveChild(node);
                node.QueueFree();
            }

            AddXAxisLabels();
            AddYAxisLabels();

            titleLabel.Text = Title;
            xAxisLabel.Text = XAxisTitle;

            graphContent.AxisDivisions = new Vector2(xAxis.GetChildCount(), yAxis.GetChildCount());

            UpdateControls();
        }

        protected virtual void AddXAxisLabels()
        {
            XAxisLabels.ForEach(label => xAxis.AddChild(new Label()
            {
                Text = label.label,
                SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                Modulate = label.color
            }));
        }

        protected virtual void AddYAxisLabels()
        {
            // reverse these so they look right on the graph
            foreach (AxisLabel label in YAxisLabels.Reverse<AxisLabel>())
            {
                yAxis.AddChild(new Label()
                {
                    Text = label.label,
                    SizeFlagsVertical = (int)SizeFlags.ExpandFill,
                    Valign = Label.VAlign.Bottom,
                    Modulate = label.color
                });
            }
        }

        public void UpdateControls()
        {
            graphContent.Points = GetPoints();
            graphContent.Update();
        }

        protected virtual List<Vector2> GetPoints()
        {
            return new();
        }

    }
}