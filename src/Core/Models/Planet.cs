using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Godot;
using log4net;
using System.ComponentModel;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Planet : MapObject, SerializableMapObject, ICargoHolder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Planet));
        public const int UnlimitedFuel = -1;

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

        [JsonIgnore]
        public int AvailableCapacity { get => int.MaxValue; }

        [JsonIgnore]
        public int Fuel
        {
            get => Starbase != null ? UnlimitedFuel : 0;
            set
            {
                // ignore setting fuel on a planet
            }
        }
        public ProductionQueue ProductionQueue { get; set; }

        public int Mines { get; set; }
        public int MaxMines { get => (Population > 0 && Player != null) ? Population * Player.Race.NumMines / 10000 : 0; }
        public int MaxPossibleMines { get => (Population > 0 && Player != null) ? GetMaxPopulation(Player.Race, Player.Rules) * Player.Race.NumMines / 10000 : 0; }
        public int Factories { get; set; }
        public int MaxFactories { get => (Population > 0 && Player != null) ? Population * Player.Race.NumFactories / 10000 : 0; }
        public int MaxPossibleFactories { get => (Population > 0 && Player != null) ? GetMaxPopulation(Player.Race, Player.Rules) * Player.Race.NumFactories / 10000 : 0; }

        public int Defenses { get; set; }
        public int MaxDefenses { get => (Population > 0 && Player != null) ? 100 : 0; }
        public bool ContributesOnlyLeftoverToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        [DefaultValue(Unexplored)]
        public int ReportAge { get; set; } = Unexplored;
        // true if the player has remote mined this planet
        public bool RemoteMined { get; set; } = false;
        public bool Explored { get => ReportAge != Unexplored; }
        public bool Uninhabited { get => Owner == null; }

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
        /// <returns></returns>
        public float PopulationDensity
        {
            get
            {
                if (Player != null)
                {
                    var rules = Player.Rules;
                    return Population > 0 ? (float)Population / GetMaxPopulation(Player.Race, rules) : 0;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Grow the planet by some grow amount
        /// </summary>
        public void Grow()
        {
            Population += GrowthAmount;
        }

        /// <summary>
        /// The amount the population for this planet will grow next turn
        /// </summary>
        /// <returns></returns>
        public int GrowthAmount
        {
            get
            {
                var race = Player?.Race;
                var rules = Player?.Rules;
                if (race != null && rules != null && Hab is Hab hab)
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
        }

        /// <summary>
        /// The mineral output of this planet if it is owned
        /// </summary>
        /// <returns></returns>
        public Mineral MineralOutput
        {
            get
            {
                var race = Player?.Race;
                if (race != null)
                {
                    return GetMineralOutput(Mines, race.MineOutput);
                }
                return Mineral.Empty;
            }
        }

        /// <summary>
        /// Get the amount of minerals this planet outputs for a a certain number of mines, given a race mineOutput
        /// </summary>
        /// <param name="mineOutput"></param>
        /// <returns></returns>
        public Mineral GetMineralOutput(int numMines, int mineOutput = 10)
        {
            return new Mineral(
                MineralsPerYear(MineralConcentration.Ironium, numMines, mineOutput),
                MineralsPerYear(MineralConcentration.Boranium, numMines, mineOutput),
                MineralsPerYear(MineralConcentration.Germanium, numMines, mineOutput)
            );

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
        public float DefenseCoverage
        {
            get
            {
                if (Player != null)
                {
                    return GetDefenseCoverage(Player.GetBestDefense());
                }
                return 0;
            }
        }

        /// <summary>
        /// Get the defense coverage for this planet against smart bombs, assuming it has a player
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float SmartDefenseCoverage
        {
            get
            {
                if (Player != null)
                {
                    return GetDefenseCoverage(Player.GetBestDefense());
                }
                return 0;
            }
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
        /// Get the defense coverage for this planet for a given defense type
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverageSmart(TechDefense defense)
        {
            if (Defenses > 0 && defense != null)
            {
                return (float)(1.0 - (Math.Pow((1 - (defense.DefenseCoverage / 100 * Player.Rules.SmartDefenseCoverageFactor)), Mathf.Clamp(Defenses, 0, MaxDefenses))));
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
        public bool AttemptTransfer(Cargo transfer, int fuel)
        {
            if (fuel > 0 || fuel < 0 && Starbase == null)
            {
                // fleets can't deposit fuel onto a planet, or take fuel from a planet without a starbase
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