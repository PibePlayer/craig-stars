
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

        public override void _Ready()
        {
            planetSummaryContainer = (PlanetSummaryContainer)FindNode("PlanetSummaryContainer");
            unknownPlanetContainer = (Control)FindNode("UnknownPlanetContainer");
            fleetSummaryContainer = (Control)FindNode("FleetSummaryContainer");
            nameLabel = (Label)FindNode("Name");

            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.TurnPassedEvent -= OnTurnPassed;
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