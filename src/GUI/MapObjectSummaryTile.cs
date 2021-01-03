using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class MapObjectSummaryTile : Control
    {
        Texture planetTexture;
        Texture fleetTexture;
        TextureRect textureRect;

        public MapObject MapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObject mapObject;

        Label nameLabel;

        public override void _Ready()
        {
            planetTexture = ResourceLoader.Load<Texture>("res://assets/gui/planet/Planet01.jpg");
            fleetTexture = ResourceLoader.Load<Texture>("res://assets/gui/tech/ShipHull/Scout0004.png");

            nameLabel = FindNode("Name") as Label;
            textureRect = FindNode("TextureRect") as TextureRect;


            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        }

        // public override object GetDragData(Vector2 position)
        // {
        //     var cpb = new ColorPickerButton();

        //     cpb.Color = Colors.Red;

        //     cpb.RectSize = new Vector2(50, 50);

        //     SetDragPreview(cpb);
        //     return Colors.Red;
        // }

        void OnMapObjectActivated(MapObject mapObject)
        {
            MapObject = mapObject;
        }

        void UpdateControls()
        {
            if (MapObject != null)
            {
                nameLabel.Text = mapObject.ObjectName;
                if (MapObject is Planet)
                {
                    textureRect.Texture = planetTexture;
                }
                else
                {
                    textureRect.Texture = fleetTexture;
                }
            }
            else
            {
                nameLabel.Text = "Unknown";
            }
        }
    }
}
