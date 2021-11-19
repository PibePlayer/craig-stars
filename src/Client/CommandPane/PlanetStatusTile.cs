using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class PlanetStatusTile : PlanetTile
    {
        [Inject] protected PlayerTechService playerTechService;
        
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
            this.ResolveDependencies();
            base._Ready();
            population = FindNode("Population") as Label;
            resources = FindNode("Resources") as Label;
            scannerType = FindNode("ScannerType") as Label;
            scannerRange = FindNode("ScannerRange") as Label;
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
                resources.Text = $"{planetService.GetResourcesPerYearAvailable(CommandedPlanet.Planet, Me):n0} of {planetService.GetResourcesPerYear(CommandedPlanet.Planet, Me):n0}";
                defenses.Text = $"{CommandedPlanet.Planet.Defenses:n0} of {planetService.GetMaxDefenses(CommandedPlanet.Planet, Me):n0}";
                var defense = playerTechService.GetBestDefense(Me);
                defenseType.Text = defense?.Name;
                defenseCoverage.Text = $"{planetService.GetDefenseCoverage(CommandedPlanet.Planet, Me):P1}";
                if (CommandedPlanet.Planet.Scanner)
                {
                    var scanner = playerTechService.GetBestPlanetaryScanner(Me);
                    scannerType.Text = $"{scanner.Name}";
                    if (scanner.ScanRangePen > 0)
                    {
                        scannerRange.Text = $"{scanner.ScanRange * Me.Race.Spec.ScanRangeFactor}/{scanner.ScanRangePen} l.y.";
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
