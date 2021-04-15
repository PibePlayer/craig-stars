
using Godot;
using System;
using CraigStars.Singletons;
using System.Collections.Generic;

namespace CraigStars
{
    public class SelectionSummaryPane : MarginContainer
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        Player Me { get => PlayersManager.Me; }

        public MapObjectSprite MapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        List<MapObject> mapObjectsHere = new List<MapObject>();
        int mapObjectIndex = 0;

        PlanetSummaryContainer planetSummaryContainer;
        Control unknownPlanetContainer;
        Control fleetSummaryContainer;
        Label nameLabel;
        Button otherFleetsButton;

        public override void _Ready()
        {
            planetSummaryContainer = GetNode<PlanetSummaryContainer>("VBoxContainer/PlanetSummaryContainer");
            unknownPlanetContainer = GetNode<Control>("VBoxContainer/UnknownPlanetContainer");
            fleetSummaryContainer = GetNode<Control>("VBoxContainer/FleetSummaryContainer");
            nameLabel = GetNode<Label>("VBoxContainer/Title/Name");
            otherFleetsButton = GetNode<Button>("VBoxContainer/Title/OtherFleetsButton");

            otherFleetsButton.Connect("pressed", this, nameof(OnOtherFleetsButtonPressed));

            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnOtherFleetsButtonPressed()
        {
            if (mapObject != null)
            {
                mapObjectIndex++;
                if (mapObjectsHere.Count > 0)
                {
                    if (mapObjectIndex >= mapObjectsHere.Count - 1)
                    {
                        mapObjectIndex = 0;
                    }

                    var mapObject = mapObjectsHere[mapObjectIndex];
                    if (mapObject.Player == Me)
                    {
                        // select the first orbiting fleet
                        Signals.PublishCommandMapObjectEvent(mapObject);
                    }
                    else
                    {
                        // select the first orbiting fleet
                        Signals.PublishSelectMapObjectEvent(mapObject);
                    }

                }
            }
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            MapObject = mapObject;
            mapObjectsHere.Clear();
            if (MapObject != null && Me.MapObjectsByLocation.TryGetValue(MapObject.MapObject.Position, out var mapObjectsAtLocation))
            {
                mapObjectsHere.AddRange(mapObjectsAtLocation);
            }
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            UpdateControls();
        }

        void UpdateControls()
        {

            if (MapObject != null)
            {
                nameLabel.Text = $"{MapObject.ObjectName} Summary";

                if (MapObject.MapObject is Planet planet)
                {
                    if (planet.Explored && planet.Hab is Hab hab)
                    {
                        planetSummaryContainer.Visible = true;
                        unknownPlanetContainer.Visible = false;
                    }
                    else
                    {
                        planetSummaryContainer.Visible = false;
                        unknownPlanetContainer.Visible = true;
                    }
                    fleetSummaryContainer.Visible = false;
                }
                else if (MapObject.MapObject is Fleet fleet)
                {
                    planetSummaryContainer.Visible = false;
                    unknownPlanetContainer.Visible = false;
                    fleetSummaryContainer.Visible = true;
                }
                else
                {
                    nameLabel.Text = "Unknown";
                    fleetSummaryContainer.Visible = false;
                    planetSummaryContainer.Visible = false;
                    unknownPlanetContainer.Visible = false;
                }
            }
        }
    }
}