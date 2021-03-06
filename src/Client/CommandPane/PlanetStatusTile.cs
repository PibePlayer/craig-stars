using CraigStars.Singletons;
using Godot;

namespace CraigStars
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
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                population.Text = $"{ActivePlanet.Planet.Population:n0}";
                resources.Text = $"{ActivePlanet.Planet.ResourcesPerYearAvailable:n0} of {ActivePlanet.Planet.ResourcesPerYear:n0}";
                defenses.Text = $"{ActivePlanet.Planet.Defenses:n0} of {ActivePlanet.Planet.MaxDefenses:n0}";
                var defense = ActivePlanet.Planet.Player?.GetBestDefense();
                defenseType.Text = defense?.Name;
                defenseCoverage.Text = $"{ActivePlanet.Planet?.DefenseCoverage:P1}";
                if (ActivePlanet.Planet.Scanner)
                {
                    var scanner = ActivePlanet.Planet.Player.GetBestPlanetaryScanner();
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
