using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class PlanetSummaryContainer : VBoxContainer
    {
        [Inject] protected PlanetService planetService;

        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        List<int> scales = new List<int>() { 100, 500, 1000, 2500, 5000, 7500, 10_000, 20_000, 30_000 };

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
        Container scaleLabels;
        PopupMenu scalePopupMenu;

        PopulationTooltip populationTooltip;
        MineralTooltip mineralTooltip;
        GravityTooltip gravityTooltip;
        TemperatureTooltip temperatureTooltip;
        RadiationTooltip radiationTooltip;

        public override void _Ready()
        {
            this.ResolveDependencies();
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
            scaleLabels = (Container)FindNode("ScaleLabels");
            scalePopupMenu = (PopupMenu)FindNode("ScalePopupMenu");

            populationTooltip = GetNode<PopulationTooltip>("CanvasLayer/PopulationTooltip");
            mineralTooltip = GetNode<MineralTooltip>("CanvasLayer/MineralTooltip");
            gravityTooltip = GetNode<GravityTooltip>("CanvasLayer/GravityTooltip");
            temperatureTooltip = GetNode<TemperatureTooltip>("CanvasLayer/TemperatureTooltip");
            radiationTooltip = GetNode<RadiationTooltip>("CanvasLayer/RadiationTooltip");

            valueLabel.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });
            populationLabel.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });
            gravHabBar.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { gravityTooltip });
            tempHabBar.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { temperatureTooltip });
            radHabBar.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { radiationTooltip });

            ironiumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Ironium });
            boraniumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Boranium });
            germaniumMineralBar.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Germanium });

            Connect("visibility_changed", this, nameof(OnVisible));
            EventManager.MapObjectSelectedEvent += OnMapObjectSelected;

            // add the scales
            scales.ForEach(scale => scalePopupMenu.AddRadioCheckItem($"{scale}kT", scale));
            scales.Each((scale, index) => scalePopupMenu.SetItemChecked(index, scale == Me.UISettings.MineralScale));
            scalePopupMenu.Show();
            scalePopupMenu.Hide(); // force resize

            scalePopupMenu.Connect("id_pressed", this, nameof(OnScalePopupMenuIdPressed));
            scaleLabels.Connect("gui_input", this, nameof(OnScaleLabelsGuiInput));
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

        void OnScalePopupMenuIdPressed(int id)
        {
            Me.UISettings.MineralScale = id;
            // save the scale to the UI
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
            UpdateMineralScale();
        }

        void OnScaleLabelsGuiInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                var mousePos = GetGlobalMousePosition();
                var yPos = mousePos.y - scalePopupMenu.RectSize.y;
                scalePopupMenu.RectPosition = new Vector2(mousePos.x, Mathf.Clamp(yPos, 0, GetViewportRect().Size.y - scalePopupMenu.RectSize.y));
                scalePopupMenu.ShowModal();
            }
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

        void UpdateMineralScale()
        {
            ironiumMineralBar.Scale = Me.UISettings.MineralScale;
            boraniumMineralBar.Scale = Me.UISettings.MineralScale;
            germaniumMineralBar.Scale = Me.UISettings.MineralScale;

            // toggle this checkbox item
            scales.Each((scale, index) => scalePopupMenu.SetItemChecked(index, scale == Me.UISettings.MineralScale));

            int labelValue = 0;
            int numLabels = scaleLabels.GetChildCount();
            foreach (Node node in scaleLabels.GetChildren())
            {
                var label = node as Label;
                if (label != null)
                {
                    label.Text = labelValue.ToString();
                    labelValue += (int)(Me.UISettings.MineralScale / (float)numLabels + .5f);
                }
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
                    Hab terraformedHab = planet.Hab.Value + planet.Spec.TerraformAmount;
                    if (terraformedHab != hab)
                    {
                        terraformHabValue = race.GetPlanetHabitability(terraformedHab);
                    }

                    valueLabel.Text = $"{habValue}%";
                    if (habValue >= 0)
                    {
                        valueLabel.Modulate = GUIColorsProvider.Colors.HabitablePlanetTextColor;
                    }
                    else
                    {
                        valueLabel.Modulate = GUIColorsProvider.Colors.UninhabitablePlanetTextColor;
                    }
                    if (terraformHabValue != habValue)
                    {
                        valueTerraformedLabel.Visible = true;
                        valueTerraformedLabel.Text = $"({terraformHabValue}%)";
                        if (terraformHabValue >= 0)
                        {
                            valueTerraformedLabel.Modulate = GUIColorsProvider.Colors.HabitablePlanetTextColor;
                        }
                        else
                        {
                            valueTerraformedLabel.Modulate = GUIColorsProvider.Colors.UninhabitablePlanetTextColor;
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
                    else if (planet.Owned)
                    {
                        ownerLabel.Text = planet.RacePluralName;
                        ownerLabel.Modulate = GameInfo.Players[planet.PlayerNum].Color;
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

                    ironiumMineralBar.MiningRate = planet.Spec.MineralOutput.Ironium;
                    boraniumMineralBar.MiningRate = planet.Spec.MineralOutput.Boranium;
                    germaniumMineralBar.MiningRate = planet.Spec.MineralOutput.Germanium;

                    UpdateMineralScale();
                }
            }
        }
    }
}