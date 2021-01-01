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
                population.Text = $"{ActivePlanet.Population:n0}";
                resources.Text = $"{ActivePlanet.ResourcesPerYearAvailable:n0} of {ActivePlanet.ResourcesPerYear:n0}";
                defenses.Text = $"{ActivePlanet.Defenses:n0} of {ActivePlanet.MaxDefenses:n0}";
            }
        }
    }
}
