using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Godot;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Planet : MapObject, SerializableMapObject, ICargoHolder
    {
        public const int Unexplored = -1;

        #region Scannable Stats

        public Hab? Hab { get; set; }
        public Mineral MineralConcentration { get; set; }

        [JsonIgnore]
        public int Population
        {
            get => Cargo.Colonists * 100;
            set
            {
                Cargo = Cargo.WithColonists(value / 100);
            }
        }

        [JsonIgnore]
        public List<Fleet> OrbitingFleets { get; set; } = new List<Fleet>();
        public Starbase Starbase { get; set; }
        public bool HasStarbase { get => Starbase != null; }

        #endregion

        #region Planet Makeup

        public Mineral MineYears { get; set; }
        public Cargo Cargo { get; set; }
        public ProductionQueue ProductionQueue { get; set; }

        public int Mines { get; set; }
        public int MaxMines { get => (Population > 0 && Player != null) ? Population * Player.Race.NumMines / 10000 : 0; }
        public int Factories { get; set; }
        public int MaxFactories { get => (Population > 0 && Player != null) ? Population * Player.Race.NumFactories / 10000 : 0; }

        public int Defenses { get; set; }
        public int MaxDefenses { get => (Population > 0 && Player != null) ? Population * 10 / 10000 : 0; }
        public bool ContributesOnlyLeftoverToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        public int ReportAge { get; set; } = Unexplored;
        public bool Explored { get => ReportAge != Unexplored; }
        public bool Uninhabited { get => Player == null; }

        #endregion

        /// <summary>
        /// The client has null values for these, but the server needs to start with
        /// an empty planet
        /// </summary>
        public void InitEmptyPlanet()
        {
            Hab = new Hab();
            MineralConcentration = new Mineral();
            Cargo = new Cargo();
            ProductionQueue = new ProductionQueue();
            MineYears = new Mineral();
        }

        /// <summary>
        /// Update this planet report with data from the game planet
        /// If this is our planet, update all stats
        /// </summary>
        /// <param name="planet">The game planet to copy data from</param>
        public void UpdatePlanetReport(Planet planet)
        {
            Cargo = Cargo.Empty;

            MineralConcentration = planet.MineralConcentration;
            Hab = planet.Hab;
            Population = planet.Population;
            ReportAge = 0;
        }

        /// <summary>
        /// Update this player's copy of their own planet, for the UI
        /// </summary>
        /// <param name="planet"></param>
        public void UpdatePlayerPlanet(Planet planet, Rules rules)
        {
            Cargo = planet.Cargo;
            MineYears = planet.MineYears;
            Mines = planet.Mines;
            Factories = planet.Factories;
            Defenses = planet.Defenses;
            Scanner = planet.Scanner;
            Homeworld = planet.Homeworld;
            ContributesOnlyLeftoverToResearch = planet.ContributesOnlyLeftoverToResearch;

            ProductionQueue = planet.ProductionQueue;

            if (planet.HasStarbase)
            {
                Starbase = new Starbase()
                {
                    Guid = planet.Starbase.Guid,
                    Position = planet.Starbase.Position,
                    Name = planet.Starbase.Name,
                    RaceName = planet.Starbase.Player.Race.Name,
                    RacePluralName = planet.Starbase.Player.Race.PluralName,
                    Player = Player,
                };
                Starbase.Tokens.AddRange(planet.Starbase.Tokens);
                Starbase.ComputeAggregate(rules);
            }
        }

        /// <summary>
        /// The max population for the race on this planet
        /// TODO: support this later
        /// /// </summary>
        /// <returns></returns>
        public int GetMaxPopulation(Race race, Rules rules)
        {
            if (Hab is Hab planetHab)
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
                var hab = race.GetPlanetHabitability(planetHab);
                return (int)(rules.MaxPopulation * factor * hab / 100);
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// Get the population density of this planet, as a float, i.e. 100k out of 1 million max is .1f density
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public float GetPopulationDensity(Rules rules)
        {
            return Population > 0 ? (float)Population / GetMaxPopulation(Player.Race, rules) : 0;
        }

        /// <summary>
        /// Grow the planet by some grow amount
        /// </summary>
        /// <param name="rules"></param>
        public void Grow(Rules rules)
        {
            Population += GetGrowthAmount(rules);
        }

        /// <summary>
        /// The amount the population for this planet will grow next turn
        /// </summary>
        /// <returns></returns>
        public int GetGrowthAmount(Rules rules)
        {
            var race = Player?.Race;
            if (race != null && Hab is Hab hab)
            {
                double capacity = (double)(Population / GetMaxPopulation(race, rules));
                int popGrowth = (int)((double)(Population) * (race.GrowthRate / 100.0) * ((double)(race.GetPlanetHabitability(hab)) / 100.0));

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
                if (Player != null && ContributesOnlyLeftoverToResearch)
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
                if (Player != null && ContributesOnlyLeftoverToResearch)
                {
                    return (int)(ResourcesPerYear * (Player.ResearchAmount / 100.0));
                }
                else
                {
                    return ResourcesPerYear;
                }

            }
        }

        /// <summary>
        /// Get the defense coverage for this planet, assuming it has a player
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverage(ITechStore techStore)
        {
            return GetDefenseCoverage(Player?.GetBestDefense(techStore));
        }

        /// <summary>
        /// Get the defense coverage for this planet for a given defense type
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverage(TechDefense defense)
        {
            if (Defenses > 0 && defense != null)
            {
                return (float)(1.0 - (Math.Pow((1 - (defense.DefenseCoverage / 100)), Mathf.Clamp(Defenses, 0, MaxDefenses))));
            }
            return 0;
        }

        /// <summary>
        /// Return true if this is a planet that the current player can build
        /// </summary>
        /// <param name="mass"></param>
        /// <returns></returns>
        public bool CanBuild(Player player, int mass)
        {
            if (OwnedBy(player) && HasStarbase)
            {
                var dockCapacity = Starbase?.DockCapacity;
                return dockCapacity == TechHull.UnlimitedSpaceDock || dockCapacity >= mass;
            }
            return false;
        }


        /// <summary>
        /// Attempt to transfer cargo to/from this planet
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns>true if we have minerals we can transfer</returns>
        public bool AttemptTransfer(Cargo transfer)
        {
            if (transfer.Fuel != 0)
            {
                // ignore fuel requests to planets
                return false;
            }

            var result = Cargo + transfer;
            if (result >= 0)
            {
                // The transfer doesn't leave us with 0 minerals, so allow it
                Cargo = result;
                return true;
            }
            return false;
        }



    }
}