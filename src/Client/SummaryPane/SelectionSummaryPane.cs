using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class SelectionSummaryPane : MarginContainer
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        public MapObjectSprite MapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObjectSprite mapObject;

        Race race;

        Control planetContainer;
        Control unknownPlanetContainer;
        Label nameLabel;
        Label valueLabel;
        Label reportAgeLabel;

        HabBar gravHabBar;
        HabBar tempHabBar;
        HabBar radHabBar;

        MineralBar ironiumMineralBar;
        MineralBar boraniumMineralBar;
        MineralBar germaniumMineralBar;

        public override void _Ready()
        {
            planetContainer = FindNode("PlanetContainer") as Control;
            unknownPlanetContainer = FindNode("UnknownPlanetContainer") as Control;
            nameLabel = FindNode("Name") as Label;
            valueLabel = FindNode("Value") as Label;
            reportAgeLabel = FindNode("ReportAge") as Label;

            race = PlayersManager.Instance.Me.Race;

            gravHabBar = FindNode("GravHabBar") as HabBar;
            tempHabBar = FindNode("TempHabBar") as HabBar;
            radHabBar = FindNode("RadHabBar") as HabBar;

            gravHabBar.Low = race.HabLow.grav;
            tempHabBar.Low = race.HabLow.temp;
            radHabBar.Low = race.HabLow.rad;

            gravHabBar.High = race.HabHigh.grav;
            tempHabBar.High = race.HabHigh.temp;
            radHabBar.High = race.HabHigh.rad;

            ironiumMineralBar = FindNode("IroniumMineralBar") as MineralBar;
            boraniumMineralBar = FindNode("BoraniumMineralBar") as MineralBar;
            germaniumMineralBar = FindNode("GermaniumMineralBar") as MineralBar;

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

        void OnTurnPassed(int year)
        {
            UpdateControls();
        }

        void UpdateControls()
        {
            if (MapObject != null)
            {
                nameLabel.Text = $"{MapObject.ObjectName} Summary";

                var planet = MapObject?.MapObject as Planet;
                if (planet != null && race != null)
                {
                    if (planet.Explored && planet.Hab is Hab hab)
                    {
                        planetContainer.Visible = true;
                        unknownPlanetContainer.Visible = false;

                        int habValue = race.GetPlanetHabitability(hab);
                        valueLabel.Text = $"{habValue}%";
                        // TODO: Add terraforming
                        if (habValue > 0)
                        {
                            valueLabel.Modulate = GUIColors.HabitablePlanetTextColor;
                        }
                        else
                        {
                            valueLabel.Modulate = GUIColors.UninhabitablePlanetTextColor;
                        }

                        if (planet.ReportAge == 0)
                        {
                            reportAgeLabel.Text = "Report is current";
                        }
                        else if (planet.ReportAge == 1)
                        {
                            reportAgeLabel.Text = "Report 1 year old";
                        }
                        else
                        {
                            reportAgeLabel.Text = $"Report {planet.ReportAge} years old";
                        }

                        gravHabBar.HabValue = hab.grav;
                        tempHabBar.HabValue = hab.temp;
                        radHabBar.HabValue = hab.rad;

                        ironiumMineralBar.Concentration = planet.MineralConcentration.Ironium;
                        boraniumMineralBar.Concentration = planet.MineralConcentration.Boranium;
                        germaniumMineralBar.Concentration = planet.MineralConcentration.Germanium;

                        ironiumMineralBar.Surface = planet.Cargo.Ironium;
                        boraniumMineralBar.Surface = planet.Cargo.Boranium;
                        germaniumMineralBar.Surface = planet.Cargo.Germanium;
                    }
                    else
                    {
                        planetContainer.Visible = false;
                        unknownPlanetContainer.Visible = true;
                    }
                }
                else
                {
                    nameLabel.Text = "Unknown";
                    planetContainer.Visible = false;
                    unknownPlanetContainer.Visible = false;
                }
            }
        }
    }
}