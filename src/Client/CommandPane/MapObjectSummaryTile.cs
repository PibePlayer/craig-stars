using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class MapObjectSummaryTile : Control
    {
        public MapObjectSprite CommandedMapObject
        {
            get => mapObject;
            set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        Label titleLabel;
        TextureRect textureRect;
        Button nextButton;
        Button prevButton;
        Button renameButton;
        LineEditDialog lineEditDialog;

        public override void _Ready()
        {
            titleLabel = (Label)FindNode("TitleLabel");
            textureRect = (TextureRect)FindNode("TextureRect");
            nextButton = (Button)FindNode("NextButton");
            prevButton = (Button)FindNode("PrevButton");
            renameButton = (Button)FindNode("RenameButton");
            lineEditDialog = GetNode<LineEditDialog>("CanvasLayer/LineEditDialog");

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            renameButton.Connect("pressed", this, nameof(OnRenameButtonPressed));

            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
            }
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
                lineEditDialog.PopupCentered(fleetSprite.Fleet.BaseName, (text) =>
                {
                    fleetSprite.Fleet.BaseName = text;
                    titleLabel.Text = CommandedMapObject.ObjectName;
                });
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
                renameButton.Visible = false;
                titleLabel.Text = CommandedMapObject.ObjectName;
                if (CommandedMapObject is PlanetSprite planetSprite)
                {
                    textureRect.Texture = TextureLoader.Instance.FindTexture(planetSprite.Planet);
                }
                else if (CommandedMapObject is FleetSprite fleetSprite)
                {
                    renameButton.Visible = true;
                    textureRect.Texture = TextureLoader.Instance.FindTexture(fleetSprite.Fleet.Tokens[0].Design);
                }
            }
            else
            {
                titleLabel.Text = "Unknown";
            }
        }
    }
}
