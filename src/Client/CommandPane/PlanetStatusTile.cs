using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class PlanetStatusTile : PlanetTile
    {
        Label population;
        Label resources;
        Label scannerType;
        Label scannerRange;
        HSeparator defenseHSeparator;
        GridContainer defensesGrid;
        Label defenses;
        Label defenseType;
        Label defenseCoverage;

        PopulationTooltip populationTooltip;
        ResourcesTooltip resourcesTooltip;

        public override void _Ready()
        {
            base._Ready();
            population = FindNode("Population") as Label;
            resources = FindNode("Resources") as Label;
            scannerType = FindNode("ScannerType") as Label;
            scannerRange = FindNode("ScannerRange") as Label;
            defenseHSeparator = FindNode("DefenseHSeparator") as HSeparator;
            defensesGrid = FindNode("DefensesGrid") as GridContainer;
            defenses = FindNode("Defenses") as Label;
            defenseType = FindNode("DefenseType") as Label;
            defenseCoverage = FindNode("DefenseCoverage") as Label;
            populationTooltip = GetNode<PopulationTooltip>("VBoxContainer/Controls/CanvasLayer/PopulationTooltip");
            population.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });
            resourcesTooltip = GetNode<ResourcesTooltip>("VBoxContainer/Controls/CanvasLayer/ResourcesTooltip");
            resources.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { resourcesTooltip });

        }

        void OnTooltipGuiInput(InputEvent @event, CSTooltip tooltip)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                tooltip.ShowAtMouse(CommandedPlanet?.Planet);
            }
            else if (@event.IsActionReleased("ui_select"))
            {
                tooltip.Hide();
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedPlanet != null)
            {
                population.Text = $"{CommandedPlanet.Planet.Population:n0}";
                resources.Text = $"{CommandedPlanet.Planet.Spec.ResourcesPerYearAvailable:n0} of {CommandedPlanet.Planet.Spec.ResourcesPerYear:n0}";

                if (Me.Race.Spec.CanBuildDefenses)
                {
                    defenseHSeparator.Visible = true;
                    defensesGrid.Visible = true;

                    defenses.Text = $"{CommandedPlanet.Planet.Defenses:n0} of {CommandedPlanet.Planet.Spec.MaxDefenses:n0}";
                    defenseType.Text = $"{CommandedPlanet.Planet.Spec.Defense.Name}";
                    defenseCoverage.Text = $"{CommandedPlanet.Planet.Spec.DefenseCoverage:P1}";
                }
                else
                {
                    defenseHSeparator.Visible = false;
                    defensesGrid.Visible = false;
                }

                if (CommandedPlanet.Planet.Scanner)
                {
                    if (Me.Race.Spec.InnateScanner)
                    {
                        scannerType.Text = $"Innate";
                    }
                    else
                    {
                        scannerType.Text = $"{CommandedPlanet.Planet.Spec.Scanner.Name}";
                    }

                    if (CommandedPlanet.Planet.Spec.ScanRangePen > 0)
                    {
                        scannerRange.Text = $"{CommandedPlanet.Planet.Spec.ScanRange}/{CommandedPlanet.Planet.Spec.ScanRangePen} l.y.";
                    }
                    else
                    {
                        scannerRange.Text = $"{CommandedPlanet.Planet.Spec.ScanRange} l.y.";
                    }

                }
                else
                {
                    scannerType.Text = $"(non)";
                    scannerRange.Text = $"";
                }
            }
        }
    }
}
