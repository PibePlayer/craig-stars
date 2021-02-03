using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars
{
    public class MapObjectSummaryTile : Control
    {
        public MapObjectSprite ActiveMapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        Label nameLabel;
        TextureRect textureRect;
        Button nextButton;
        Button prevButton;
        Button renameButton;

        public override void _Ready()
        {
            nameLabel = FindNode("Name") as Label;
            textureRect = FindNode("TextureRect") as TextureRect;
            nextButton = FindNode("NextButton") as Button;
            prevButton = FindNode("PrevButton") as Button;
            renameButton = FindNode("RenameButton") as Button;

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            renameButton.Connect("pressed", this, nameof(OnRenameButtonPressed));

            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        }

        void OnNextButtonPressed()
        {
            Signals.PublishActiveNextMapObjectEvent();
        }

        void OnPrevButtonPressed()
        {
            Signals.PublishActivePrevMapObjectEvent();
        }

        void OnRenameButtonPressed()
        {
            if (ActiveMapObject is FleetSprite fleetSprite)
            {
                Signals.PublishRenameFleetRequestedEvent(fleetSprite);
            }
        }

        void OnMapObjectActivated(MapObjectSprite mapObject)
        {
            ActiveMapObject = mapObject;
        }

        void UpdateControls()
        {
            if (ActiveMapObject != null)
            {
                nameLabel.Text = mapObject.ObjectName;
                if (ActiveMapObject is PlanetSprite planetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(planetSprite.Planet);
                }
                else if (ActiveMapObject is FleetSprite fleetSprite)
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
