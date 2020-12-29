using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class SelectionSummaryPane : MarginContainer
    {

        public MapObject MapObject
        {
            get => mapObject; set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        MapObject mapObject;

        Race race;

        GUIColors guiColors = new GUIColors();
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
            guiColors = GD.Load("res://src/GUI/GUIColors.tres") as GUIColors;
            if (guiColors == null)
            {
                guiColors = new GUIColors();
            }

            planetContainer = FindNode("PlanetContainer") as Control;
            unknownPlanetContainer = FindNode("UnknownPlanetContainer") as Control;
            nameLabel = FindNode("Name") as Label;
            valueLabel = FindNode("Value") as Label;
            reportAgeLabel = FindNode("ReportAge") as Label;

            race = PlayersManager.Instance.Me.Race;

            gravHabBar = FindNode("GravHabBar") as HabBar;
            tempHabBar = FindNode("TempHabBar") as HabBar;
            radHabBar = FindNode("RadHabBar") as HabBar;

            gravHabBar.Low = race.HabLow.Grav;
            tempHabBar.Low = race.HabLow.Temp;
            radHabBar.Low = race.HabLow.Rad;

            gravHabBar.High = race.HabHigh.Grav;
            tempHabBar.High = race.HabHigh.Temp;
            radHabBar.High = race.HabHigh.Rad;

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

        void OnMapObjectSelected(MapObject mapObject)
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

                var planet = MapObject as Planet;
                if (planet != null && race != null)
                {
                    if (planet.Explored)
                    {
                        planetContainer.Visible = true;
                        unknownPlanetContainer.Visible = false;

                        int habValue = race.GetPlanetHabitability(planet.Hab);
                        valueLabel.Text = $"{habValue}%";
                        // TODO: Add terraforming
                        if (habValue > 0)
                        {
                            valueLabel.Modulate = guiColors.HabitablePlanetTextColor;
                        }
                        else
                        {
                            valueLabel.Modulate = guiColors.UninhabitablePlanetTextColor;
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

                        gravHabBar.HabValue = planet.Hab.Grav;
                        tempHabBar.HabValue = planet.Hab.Temp;
                        radHabBar.HabValue = planet.Hab.Rad;

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