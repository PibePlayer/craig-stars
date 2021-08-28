using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class PlanetSummaryContainer : VBoxContainer
    {
        PlanetService planetService = new();

        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        Player Me { get => PlayersManager.Me; }

        public PlanetSprite Planet
        {
            get => planet;
            set
            {
                planet = value;
                UpdateControls();
            }
        }
        PlanetSprite planet;

        Label valueLabel;
        Label valueTerraformedLabel;
        Label populationLabel;
        Label reportAgeLabel;
        Label ownerLabel;

        HabBar gravHabBar;
        HabBar tempHabBar;
        HabBar radHabBar;

        MineralBar ironiumMineralBar;
        MineralBar boraniumMineralBar;
        MineralBar germaniumMineralBar;

        PopulationTooltip populationTooltip;
        MineralTooltip mineralTooltip;

        public override void _Ready()
        {
            valueLabel = (Label)FindNode("Value");
            valueTerraformedLabel = (Label)FindNode("ValueTerraformed");
            reportAgeLabel = (Label)FindNode("ReportAge");
            populationLabel = (Label)FindNode("Population");
            ownerLabel = (Label)FindNode("Owner");

            gravHabBar = (HabBar)FindNode("GravHabBar");
            tempHabBar = (HabBar)FindNode("TempHabBar");
            radHabBar = (HabBar)FindNode("RadHabBar");

            ironiumMineralBar = (MineralBar)FindNode("IroniumMineralBar");
            boraniumMineralBar = (MineralBar)FindNode("BoraniumMineralBar");
            germaniumMineralBar = (MineralBar)FindNode("GermaniumMineralBar");

            populationTooltip = GetNode<PopulationTooltip>("CanvasLayer/PopulationTooltip");
            mineralTooltip = GetNode<MineralTooltip>("CanvasLayer/MineralTooltip");

            valueLabel.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });
            populationLabel.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });

            ironiumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Ironium });
            boraniumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Boranium });
            germaniumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Germanium });

            Connect("visibility_changed", this, nameof(OnVisible));
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

        void OnVisible()
        {
            var race = Me.Race;
            gravHabBar.Low = race.HabLow.grav;
            tempHabBar.Low = race.HabLow.temp;
            radHabBar.Low = race.HabLow.rad;

            gravHabBar.High = race.HabHigh.grav;
            tempHabBar.High = race.HabHigh.temp;
            radHabBar.High = race.HabHigh.rad;
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            Planet = mapObject as PlanetSprite;
        }

        void OnMineralGuiInput(InputEvent @event, MineralType type)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                mineralTooltip.ShowAtMouse(Planet?.Planet, type);
            }
            else if (@event.IsActionReleased("ui_select"))
            {
                mineralTooltip.Hide();
            }
        }

        void OnTooltipGuiInput(InputEvent @event, CSTooltip tooltip)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                tooltip.ShowAtMouse(Planet?.Planet);
            }
            else if (@event.IsActionReleased("ui_select"))
            {
                tooltip.Hide();
            }
        }

        void UpdateControls()
        {
            if (Planet != null)
            {
                var race = Me.Race;
                var planet = Planet.Planet;
                if (planet.Explored && planet.Hab is Hab hab)
                {
                    int habValue = race.GetPlanetHabitability(hab);
                    int terraformHabValue = habValue;
                    Hab terraformedHab = planet.Hab.Value + planetService.GetTerraformAmount(planet, Me);
                    if (terraformedHab != hab)
                    {
                        terraformHabValue = race.GetPlanetHabitability(terraformedHab);
                    }

                    valueLabel.Text = $"{habValue}%";
                    if (habValue >= 0)
                    {
                        valueLabel.Modulate = GUIColors.HabitablePlanetTextColor;
                    }
                    else
                    {
                        valueLabel.Modulate = GUIColors.UninhabitablePlanetTextColor;
                    }
                    if (terraformHabValue != habValue)
                    {
                        valueTerraformedLabel.Visible = true;
                        valueTerraformedLabel.Text = $"({terraformHabValue}%)";
                        if (terraformHabValue >= 0)
                        {
                            valueTerraformedLabel.Modulate = GUIColors.HabitablePlanetTextColor;
                        }
                        else
                        {
                            valueTerraformedLabel.Modulate = GUIColors.UninhabitablePlanetTextColor;
                        }
                    }
                    else
                    {
                        valueTerraformedLabel.Visible = false;
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

                    if (planet.Population > 0)
                    {
                        if (planet.OwnedBy(Me))
                        {
                            populationLabel.Text = $"{planet.Population:n0}";
                        }
                        else
                        {
                            populationLabel.Text = $"±{planet.Population:n0}";
                        }
                    }
                    else
                    {
                        populationLabel.Text = "uninhabited";
                    }

                    if (planet.OwnedBy(Me))
                    {
                        ownerLabel.Text = "";
                    }
                    else if (planet.Owner != null)
                    {
                        ownerLabel.Text = planet.Owner.RacePluralName;
                        ownerLabel.Modulate = planet.Owner.Color;
                    }

                    gravHabBar.HabValue = hab.grav;
                    tempHabBar.HabValue = hab.temp;
                    radHabBar.HabValue = hab.rad;

                    gravHabBar.TerraformHabValue = terraformedHab.grav;
                    tempHabBar.TerraformHabValue = terraformedHab.temp;
                    radHabBar.TerraformHabValue = terraformedHab.rad;

                    ironiumMineralBar.Concentration = planet.MineralConcentration.Ironium;
                    boraniumMineralBar.Concentration = planet.MineralConcentration.Boranium;
                    germaniumMineralBar.Concentration = planet.MineralConcentration.Germanium;

                    ironiumMineralBar.Surface = planet.Cargo.Ironium;
                    boraniumMineralBar.Surface = planet.Cargo.Boranium;
                    germaniumMineralBar.Surface = planet.Cargo.Germanium;
                }
            }
        }
    }
}