
using Godot;
using System;
using CraigStars.Singletons;
using System.Collections.Generic;

namespace CraigStars.Client
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
        Control salvageSummaryContainer;
        Control mineFieldSummaryContainer;
        Control wormholeSummaryContainer;
        Control mineralPacketSummaryContainer;
        Label nameLabel;
        Button otherFleetsButton;

        public override void _Ready()
        {
            planetSummaryContainer = GetNode<PlanetSummaryContainer>("VBoxContainer/PlanetSummaryContainer");
            unknownPlanetContainer = GetNode<Control>("VBoxContainer/UnknownPlanetContainer");
            fleetSummaryContainer = GetNode<Control>("VBoxContainer/FleetSummaryContainer");
            salvageSummaryContainer = GetNode<Control>("VBoxContainer/SalvageSummaryContainer");
            mineFieldSummaryContainer = GetNode<Control>("VBoxContainer/MineFieldSummaryContainer");
            wormholeSummaryContainer = GetNode<Control>("VBoxContainer/WormholeSummaryContainer");
            mineralPacketSummaryContainer = GetNode<Control>("VBoxContainer/MineralPacketSummaryContainer");
            nameLabel = GetNode<Label>("VBoxContainer/Title/Name");
            otherFleetsButton = GetNode<Button>("VBoxContainer/Title/OtherFleetsButton");

            otherFleetsButton.Connect("pressed", this, nameof(OnOtherFleetsButtonPressed));

            EventManager.MapObjectSelectedEvent += OnMapObjectSelected;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.MapObjectSelectedEvent -= OnMapObjectSelected;
            }
        }

        void OnOtherFleetsButtonPressed()
        {
            if (mapObject != null)
            {
                mapObjectIndex++;
                if (mapObjectsHere.Count > 0)
                {
                    if (mapObjectIndex > mapObjectsHere.Count - 1)
                    {
                        mapObjectIndex = 0;
                    }

                    var mapObject = mapObjectsHere[mapObjectIndex];
                    if (mapObject.Player == Me)
                    {
                        // select the first orbiting fleet
                        EventManager.PublishCommandMapObjectEvent(mapObject);
                    }
                    else
                    {
                        // select the first orbiting fleet
                        EventManager.PublishSelectMapObjectEvent(mapObject);
                    }
                }
                else
                {
                    mapObjectIndex = 0;
                }
            }
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            // reset otherObjectsHere if we select a mapobject with a different position (or an empty one)
            if (MapObject == null || MapObject.MapObject.Position != mapObject?.MapObject?.Position)
            {
                mapObjectIndex = 0;
                MapObject = mapObject;
                mapObjectsHere.Clear();
                if (MapObject != null && Me.MapObjectsByLocation.TryGetValue(MapObject.MapObject.Position, out var mapObjectsAtLocation))
                {
                    mapObjectsHere.AddRange(mapObjectsAtLocation);
                }
            }
            else
            {
                MapObject = mapObject;
            }
        }

        void UpdateControls()
        {
            salvageSummaryContainer.Visible = false;
            fleetSummaryContainer.Visible = false;
            planetSummaryContainer.Visible = false;
            unknownPlanetContainer.Visible = false;
            mineFieldSummaryContainer.Visible = false;
            wormholeSummaryContainer.Visible = false;
            mineralPacketSummaryContainer.Visible = false;

            if (MapObject != null)
            {
                nameLabel.Text = $"{MapObject.ObjectName} Summary";

                if (MapObject.MapObject is Planet planet)
                {
                    if (planet.Explored && planet.Hab is Hab hab)
                    {
                        planetSummaryContainer.Visible = true;
                    }
                    else
                    {
                        unknownPlanetContainer.Visible = true;
                    }
                }
                else if (MapObject.MapObject is Fleet fleet)
                {
                    fleetSummaryContainer.Visible = true;
                }
                else if (MapObject.MapObject is Salvage salvage)
                {
                    nameLabel.Text = $"Salvage Summary";
                    salvageSummaryContainer.Visible = true;
                }
                else if (MapObject.MapObject is MineField mineField)
                {
                    mineFieldSummaryContainer.Visible = true;
                    nameLabel.Text = $"{mineField.RacePluralName} Mine Field";
                }
                else if (MapObject.MapObject is Wormhole wormhole)
                {
                    wormholeSummaryContainer.Visible = true;
                    nameLabel.Text = $"Wormhole Summary";
                }
                else if (MapObject.MapObject is MineralPacket mineralPacket)
                {
                    mineralPacketSummaryContainer.Visible = true;
                    nameLabel.Text = $"Mineral Packet Summary";
                }
                else
                {
                    nameLabel.Text = "Unknown";
                    unknownPlanetContainer.Visible = true;
                }
            }
        }
    }
}