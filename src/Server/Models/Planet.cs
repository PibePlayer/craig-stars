using System.Collections.Generic;
using System.Linq;
using Godot;
using CraigStars.Singletons;

namespace CraigStars
{
    public class Planet : MapObject
    {
        public const int Unexplored = -1;

        #region Scannable Stats

        public Hab Hab { get; set; } = new Hab();
        public Mineral MineralConcentration { get; set; } = new Mineral();
        public int Population { get => population; set { population = value; Cargo.Colonists = value / 100; } }
        int population;
        public List<Fleet> OrbitingFleets { get; set; } = new List<Fleet>();

        #endregion

        #region Planet Makeup

        public Mineral MineYears { get; set; } = new Mineral();
        public Cargo Cargo { get; } = new Cargo();
        public ProductionQueue ProductionQueue { get; } = new ProductionQueue();

        public int PopulationDensity { get => Population > 0 ? Population / GetMaxPopulation(Player.Race) : 0; }

        public int Mines { get; set; }
        public int MaxMines { get => (Population > 0 && Player != null) ? Population / 10000 * Player.Race.NumMines : 0; }
        public int Factories { get; set; }
        public int MaxFactories { get => (Population > 0 && Player != null) ? Population / 10000 * Player.Race.NumFactories : 0; }

        public int Defenses { get; set; }
        public int MaxDefenses { get => (Population > 0 && Player != null) ? Population / 10000 * 10 : 0; }
        public bool ContributesToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        public int ReportAge { get; set; } = Unexplored;
        public bool Explored { get => ReportAge != Unexplored; }

        #endregion


        /// <summary>
        /// Update this planet report with data from the game planet
        /// If this is our planet, update all stats
        /// </summary>
        /// <param name="planet">The game planet to copy data from</param>
        public void UpdatePlanetReport(Planet planet)
        {
            MineralConcentration.Copy(planet.MineralConcentration);
            Hab.Copy(planet.Hab);
            Population = planet.Population;
            ReportAge = 0;
        }

        /// <summary>
        /// Update this player's copy of their own planet, for the UI
        /// </summary>
        /// <param name="planet"></param>
        public void UpdatePlayerPlanet(Planet planet)
        {
            Cargo.Copy(planet.Cargo);
            MineYears.Copy(planet.MineYears);
            Mines = planet.Mines;
            Factories = planet.Mines;
            Defenses = planet.Mines;
            Scanner = planet.Scanner;
            Homeworld = planet.Homeworld;

            ProductionQueue.Copy(planet.ProductionQueue);
        }

        /// <summary>
        /// The max population for the race on this planet
        /// TODO: support this later
        /// /// </summary>
        /// <returns></returns>
        public int GetMaxPopulation(Race race)
        {
            var factor = 1f;

            if (race.PRT == PRT.JoaT)
            {
                factor += .2f;
            }
            else if (race.PRT == PRT.HE)
            {
                factor = .5f;
            }

            if (race.HasLRT(LRT.OBRM))
            {
                factor += .1f;
            }

            // get this player's planet habitability
            var hab = race.GetPlanetHabitability(Hab);
            return (int)(UniverseSettings.MaxPopulation * factor * hab / 100);
        }

        public void Grow()
        {
            Population += GetGrowthAmount();
        }

        /// <summary>
        /// The amount the population for this planet will grow next turn
        /// </summary>
        /// <returns></returns>
        public int GetGrowthAmount()
        {
            var race = Player?.Race;
            if (race != null)
            {
                double capacity = (double)(Population / GetMaxPopulation(race));
                int popGrowth = (int)((double)(Population) * (race.GrowthRate / 100.0) * ((double)(race.GetPlanetHabitability(Hab)) / 100.0));

                if (capacity > .25)
                {
                    double crowdingFactor = 16.0 / 9.0 * (1.0 - capacity) * (1.0 - capacity);
                    popGrowth = (int)((double)(popGrowth) * crowdingFactor);
                }

                return popGrowth;

            }
            return 0;
        }

        /// <summary>
        /// The mineral output of this planet if it is owned
        /// </summary>
        /// <returns></returns>
        public Mineral GetMineralOutput()
        {
            var race = Player?.Race;
            if (race != null)
            {
                var mineOutput = race.MineOutput;
                return new Mineral(
                    MineralsPerYear(MineralConcentration.Ironium, Mines, mineOutput),
                    MineralsPerYear(MineralConcentration.Boranium, Mines, mineOutput),
                    MineralsPerYear(MineralConcentration.Germanium, Mines, mineOutput)
                );
            }
            return Mineral.Empty;
        }

        /// <summary>
        /// Get the amount of minerals mined in one year, for one type
        /// </summary>
        /// <param name="mineralConcentration">The concentration of minerals</param>
        /// <param name="mines">The number of mines on the planet</param>
        /// <param name="mineOutput">The mine output for the owner race</param>
        /// <returns>The mineral output for one year for one mineral conc</returns>
        int MineralsPerYear(int mineralConcentration, int mines, int mineOutput)
        {
            return (int)(((float)(mineralConcentration) / 100.0) * ((float)(mines) / 10.0) * (float)(mineOutput));
        }

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet generates in a year</value>
        public int ResourcesPerYear
        {
            get
            {
                if (Player != null)
                {
                    var race = Player.Race;

                    // compute resources from population
                    int resourcesFromPop = Population / race.ColonistsPerResource;

                    // compute resources from factories
                    int resourcesFromFactories = Factories * race.FactoryOutput / 10;

                    // return the sum
                    return resourcesFromPop + resourcesFromFactories;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet will contribute per year</value>
        public int ResourcesPerYearAvailable
        {
            get
            {
                if (Player != null && ContributesToResearch)
                {
                    return (int)(ResourcesPerYear * (1 - Player.ResearchAmount / 100.0));
                }
                else
                {
                    return ResourcesPerYear;
                }
            }
        }

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet will contribute per year</value>
        public int ResourcesPerYearResearch
        {
            get
            {
                if (Player != null && ContributesToResearch)
                {
                    return (int)(ResourcesPerYear * (Player.ResearchAmount / 100.0));
                }
                else
                {
                    return ResourcesPerYear;
                }

            }
        }

    }
}