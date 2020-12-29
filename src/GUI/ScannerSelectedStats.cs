using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class ScannerSelectedStats : MarginContainer
    {
        Label idLabel;
        Label xLabel;
        Label yLabel;
        Label nameLabel;

        public override void _Ready()
        {
            idLabel = FindNode("IdLabel") as Label;
            xLabel = FindNode("XLabel") as Label;
            yLabel = FindNode("YLabel") as Label;
            nameLabel = FindNode("NameLabel") as Label;


            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
        }

        void OnMapObjectSelected(MapObject mapObject)
        {
            if (mapObject != null)
            {
                if (mapObject is Planet planet)
                {
                    idLabel.Visible = true;
                    idLabel.Text = $"ID: #{planet.Id}";
                }
                else
                {
                    idLabel.Visible = false;
                }

                xLabel.Text = $"X: {mapObject.Position.x}";
                yLabel.Text = $"Y: {mapObject.Position.y}";
                nameLabel.Text = $"{mapObject.ObjectName}";
            }
        }
    }
}