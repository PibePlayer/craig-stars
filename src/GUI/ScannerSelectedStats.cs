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
        Label distanceLabel;

        MapObject selectedMapObject;
        MapObject commandedMapObject;

        public override void _Ready()
        {
            idLabel = FindNode("IdLabel") as Label;
            xLabel = FindNode("XLabel") as Label;
            yLabel = FindNode("YLabel") as Label;
            nameLabel = FindNode("NameLabel") as Label;
            distanceLabel = FindNode("DistanceLabel") as Label;


            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        }

        void OnMapObjectSelected(MapObject mapObject)
        {
            selectedMapObject = mapObject;
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

                xLabel.Text = $"X: {mapObject.Position.x:0.#}";
                yLabel.Text = $"Y: {mapObject.Position.y:0.#}";
                nameLabel.Text = $"{mapObject.ObjectName}";

                if (selectedMapObject != commandedMapObject && commandedMapObject != null)
                {
                    var dist = Math.Abs(selectedMapObject.Position.DistanceTo(commandedMapObject.Position));
                    distanceLabel.Text = $"{dist:0.#} light years from {commandedMapObject.ObjectName}";
                }
                else
                {
                    distanceLabel.Text = $"";
                }
            }
        }

        void OnMapObjectActivated(MapObject mapObject)
        {
            commandedMapObject = mapObject;
        }
    }
}