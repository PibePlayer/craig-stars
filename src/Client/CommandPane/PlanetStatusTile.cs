using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class PlanetStatusTile : PlanetTile
    {
        Label population;
        Label resources;
        Label scannerType;
        Label scannerRange;
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
            defenses = FindNode("Defenses") as Label;
            defenseType = FindNode("DefenseType") as Label;
            defenseCoverage = FindNode("DefenseCoverage") as Label;
            populationTooltip = GetNode<PopulationTooltip>("CanvasLayer/PopulationTooltip");
            population.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { populationTooltip });
            resourcesTooltip = GetNode<ResourcesTooltip>("CanvasLayer/ResourcesTooltip");
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
                resources.Text = $"{CommandedPlanet.Planet.ResourcesPerYearAvailable:n0} of {CommandedPlanet.Planet.ResourcesPerYear:n0}";
                defenses.Text = $"{CommandedPlanet.Planet.Defenses:n0} of {CommandedPlanet.Planet.MaxDefenses:n0}";
                var defense = CommandedPlanet.Planet.Player?.GetBestDefense();
                defenseType.Text = defense?.Name;
                defenseCoverage.Text = $"{CommandedPlanet.Planet?.DefenseCoverage:P1}";
                if (CommandedPlanet.Planet.Scanner)
                {
                    var scanner = CommandedPlanet.Planet.Player.GetBestPlanetaryScanner();
                    scannerType.Text = $"{scanner.Name}";
                    if (scanner.ScanRangePen > 0)
                    {
                        scannerRange.Text = $"{scanner.ScanRange}/{scanner.ScanRangePen} l.y.";
                    }
                    else
                    {
                        scannerRange.Text = $"{scanner.ScanRange} l.y.";
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
