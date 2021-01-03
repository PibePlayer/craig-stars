using Godot;
using System;

namespace CraigStars
{
    public class UniverseSettings
    {
        #region Constants

        public static int BuiltInScannerJoaTMultiplier = 20;
        public static int MaxPopulation = 1000000;

        #endregion

        /// <summary>
        /// Allow setting of the random seed used
        /// </summary>
        /// <returns></returns>
        public Random Random { get; set; } = new Random();

        /// <summary>
        /// For debugging, start with extra planets
        /// </summary>
        /// <value></value>
        public int StartWithExtraPlanets { get; set; } = 1;

        /// <summary>
        /// This is just a temporary property for development.
        /// </summary>
        public int NumPlayers = 1;

        public int StartingYear { get; set; } = 2400;
        public Size Size { get; set; } = Size.Small;
        public Density Density { get; set; } = Density.Normal;

        // Mineral settings that we don't currently modify
        public int MinHomeworldMineralConcentration { get; set; } = 30;
        public int MinExtraPlanetMineralConcentration { get; set; } = 30;
        public int MinMineralConcentration { get; set; } = 1;
        public int MinStartingMineralConcentration { get; set; } = 3;
        public int MaxStartingMineralConcentration { get; set; } = 100;
        public int MaxStartingMineralSurface { get; set; } = 1000;
        public int MinStartingMineralSurface { get; set; } = 300;
        public int MineralDecayFactor { get; set; } = 1500000;

        // Population Settings
        public int StartingPopulation { get; set; } = 25000;
        public int StartingPopulationExtraPlanet { get; set; } = 12500;
        public float LowStartingPopulationFactor { get; set; } = .7f;

        // Bulding Settings
        public int StartingMines { get; set; } = 10;
        public int StartingFactories { get; set; } = 10;
        public int StartingDefenses { get; set; } = 10;

        public int PlanetMinDistance { get; } = 15;

        public Color[] PlayerColors { get; } = new Color[] {
            new Color("c33232"),
            new Color("1f8ba7"),
            new Color("43a43e"),
            new Color("8d29cb"),
            new Color("b88628")
        };

        public int FactoryCostGermanium { get; set; } = 4;
        public Cost DefenseCost { get; set; } = new Cost(5, 5, 5, 15);
        public int MineralAlchemyCost { get; set; } = 100;
        public int MineralAlchemyLRTCost { get; set; } = 25;


        /// <summary>
        /// Get the Area of the universe
        /// </summary>
        /// <value></value>
        public int Area
        {
            get
            {
                switch (Size)
                {
                    case Size.Tiny:
                        return 400;
                    case Size.Small:
                        return 800;
                    case Size.Medium:
                        return 1200;
                    case Size.Large:
                        return 1600;
                    case Size.Huge:
                        return 2000;
                    default:
                        throw new System.ArgumentException("Unknown Size: " + Size);
                }
            }
        }

        /// <summary>
        /// Get the number of planets based on the size and density
        /// </summary>
        /// <value></value>
        public int NumPlanets
        {
            get
            {
                switch (Size)
                {
                    case Size.Huge:
                        switch (Density)
                        {
                            case (Density.Sparse): return 600;
                            case (Density.Normal): return 800;
                            case (Density.Dense): return 940;
                            case (Density.Packed): return 945;
                        }
                        break;
                    case Size.Large:
                        switch (Density)
                        {
                            case (Density.Sparse): return 384;
                            case (Density.Normal): return 512;
                            case (Density.Dense): return 640;
                            case (Density.Packed): return 910;
                        }
                        break;
                    case Size.Medium:
                        switch (Density)
                        {
                            case (Density.Sparse): return 216;
                            case (Density.Normal): return 288;
                            case (Density.Dense): return 360;
                            case (Density.Packed): return 540;
                        }
                        break;
                    case Size.Small:
                        switch (Density)
                        {
                            case (Density.Sparse): return 96;
                            case (Density.Normal): return 128;
                            case (Density.Dense): return 160;
                            case (Density.Packed): return 240;
                        }
                        break;
                    case Size.Tiny:
                        switch (Density)
                        {
                            case (Density.Sparse): return 24;
                            case (Density.Normal): return 32;
                            case (Density.Dense): return 40;
                            case (Density.Packed): return 60;
                        }
                        break;
                }
                throw new System.ArgumentException($"Unknown Size {Size} or Density {Density}");
            }
        }

        /// <summary>
        /// All homeworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral HomeWorldMineralConcentration
        {
            get
            {
                if (homeWorldMineralConcentration == null)
                {
                    homeWorldMineralConcentration = new Mineral()
                    {
                        Ironium = Random.Next(MaxStartingMineralConcentration) + MinHomeworldMineralConcentration,
                        Boranium = Random.Next(MaxStartingMineralConcentration) + MinHomeworldMineralConcentration,
                        Germanium = Random.Next(MaxStartingMineralConcentration) + MinHomeworldMineralConcentration
                    };
                }
                return homeWorldMineralConcentration;
            }
        }
        Mineral homeWorldMineralConcentration = null;

        /// <summary>
        /// All homeworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral HomeWorldSurfaceMinerals
        {
            get
            {
                if (homeWorldSurfaceMinerals == null)
                {
                    homeWorldSurfaceMinerals = new Mineral()
                    {
                        Ironium = Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface,
                        Boranium = Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface,
                        Germanium = Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface
                    };
                }
                return homeWorldSurfaceMinerals;
            }
        }
        Mineral homeWorldSurfaceMinerals = null;

        /// <summary>
        /// All extraworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral ExtraWorldSurfaceMinerals
        {
            get
            {
                if (extraWorldSurfaceMinerals == null)
                {
                    extraWorldSurfaceMinerals = new Mineral()
                    {
                        Ironium = (Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface) / 2,
                        Boranium = (Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface) / 2,
                        Germanium = (Random.Next(MaxStartingMineralSurface) + MinStartingMineralSurface) / 2
                    };
                }
                return extraWorldSurfaceMinerals;
            }
        }
        Mineral extraWorldSurfaceMinerals = null;

        // The base cost for each tech level
        public int[] TechBaseCost { get; set; } = {
            0, // everyone starts on level 0
            50,
            80,
            130,
            210,
            340,
            550,
            890,
            1440,
            2330,
            3770,
            6100,
            9870,
            13850,
            18040,
            22440,
            27050,
            31870,
            36900,
            42140,
            47590,
            53250,
            59120,
            65200,
            71490,
            77990,
            84700
        };
    }


}