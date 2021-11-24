namespace CraigStars
{
    public class PlanetSpec
    {
        public int MaxMines { get; set; }
        public int MaxPossibleMines { get; set; }
        public int MaxFactories { get; set; }
        public int MaxPossibleFactories { get; set; }
        public int MaxDefenses { get; set; }
        public float PopulationDensity { get; set; }
        public int MaxPopulation { get; set; }
        public int GrowthAmount { get; set; }
        public Mineral MineralOutput { get; set; } = new();
        public int ResourcesPerYear { get; set; }
        public int ResourcesPerYearAvailable { get; set; }
        public int ResourcesPerYearResearch { get; set; }
        public TechDefense Defense { get; set; }
        public float DefenseCoverage { get; set; }
        public float DefenseCoverageSmart { get; set; }

        public TechPlanetaryScanner Scanner { get; set; }
        public int ScanRange { get; set; }
        public int ScanRangePen { get; set; }

        public bool CanTerraform { get; set; }
        public Hab TerraformAmount { get; set; } = new();
    }
}