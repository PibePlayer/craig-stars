using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class MapObjectSummaryTile : Control, ITileContent
    {
        public event UpdateTitleAction UpdateTitleEvent;
        public event UpdateVisibilityAction UpdateVisibilityEvent;

        public MapObjectSprite CommandedMapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        TextureRect textureRect;
        Button nextButton;
        Button prevButton;
        Button renameButton;

        public override void _Ready()
        {
            textureRect = FindNode("TextureRect") as TextureRect;
            nextButton = FindNode("NextButton") as Button;
            prevButton = FindNode("PrevButton") as Button;
            renameButton = FindNode("RenameButton") as Button;

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            renameButton.Connect("pressed", this, nameof(OnRenameButtonPressed));

            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _ExitTree()
        {
            EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
        }

        void OnNextButtonPressed()
        {
            EventManager.PublishCommandNextMapObjectEvent();
        }

        void OnPrevButtonPressed()
        {
            EventManager.PublishCommandPrevMapObjectEvent();
        }

        void OnRenameButtonPressed()
        {
            if (CommandedMapObject is FleetSprite fleetSprite)
            {
                EventManager.PublishRenameFleetDialogRequestedEvent(fleetSprite);
            }
        }

        void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            CommandedMapObject = mapObject;
        }

        void UpdateControls()
        {
            if (CommandedMapObject != null)
            {
                UpdateTitleEvent?.Invoke(mapObject.ObjectName);
                if (CommandedMapObject is PlanetSprite planetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(planetSprite.Planet);
                }
                else if (CommandedMapObject is FleetSprite fleetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(fleetSprite.Fleet.Tokens[0].Design);
                }
            }
            else
            {
                UpdateTitleEvent?.Invoke("Unknown");
            }
        }
    }
}
