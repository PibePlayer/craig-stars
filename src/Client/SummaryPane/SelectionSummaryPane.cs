
using Godot;
using System;
using CraigStars.Singletons;

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
                if (mapObject is PlanetSprite planet && planet.OrbitingFleets.Count > 0)
                {
                    var fleet = planet.OrbitingFleets[0];
                    if (fleet.OwnedByMe)
                    {
                        // select the first orbiting fleet
                        Signals.PublishCommandMapObjectEvent(fleet.Fleet);
                    }
                    else
                    {
                        // select the first orbiting fleet
                        Signals.PublishMapObjectSelectedEvent(planet.OrbitingFleets[0]);
                    }
                }
                else if (mapObject is FleetSprite fleet && fleet.OtherFleets.Count > 0)
                {
                    var otherFleet = fleet.OtherFleets[0];
                    if (otherFleet.OwnedByMe)
                    {
                        // select the first orbiting fleet
                        Signals.PublishCommandMapObjectEvent(otherFleet.Fleet);
                    }
                    else
                    {
                        // select the first orbiting fleet
                        Signals.PublishMapObjectSelectedEvent(otherFleet);
                    }

                }
            }
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            MapObject = mapObject;
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