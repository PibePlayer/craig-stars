using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars
{
    public class MapObjectSummaryTile : Control
    {
        TextureRect textureRect;

        public MapObjectSprite MapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        Label nameLabel;

        public override void _Ready()
        {
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

        void OnMapObjectActivated(MapObjectSprite mapObject)
        {
            MapObject = mapObject;
        }

        void UpdateControls()
        {
            if (MapObject != null)
            {
                nameLabel.Text = mapObject.ObjectName;
                if (MapObject is PlanetSprite planetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(planetSprite.Planet);
                }
                else if (MapObject is FleetSprite fleetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(fleetSprite.Fleet.Tokens[0].Design);
                }
            }
            else
            {
                nameLabel.Text = "Unknown";
            }
        }
    }
}
