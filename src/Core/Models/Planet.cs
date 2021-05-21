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
        static CSLog log = LogProvider.GetLogger(typeof(Planet));
        public const int UnlimitedFuel = -1;

        #region Scannable Stats

        public Hab? Hab { get; set; }
        public Hab? BaseHab { get; set; }
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


        [JsonIgnore] public List<Fleet> OrbitingFleets { get; set; } = new List<Fleet>();

        public Starbase Starbase { get; set; }
        public int PacketSpeed { get; set; } = 5;
        [JsonProperty(IsReference = true)]
        public Planet PacketTarget { get; set; }

        [JsonIgnore] public bool HasStarbase { get => Starbase != null; }
        [JsonIgnore] public bool HasMassDriver { get => Starbase != null && Starbase.Aggregate.HasMassDriver; }
        [JsonIgnore] public bool HasStargate { get => Starbase != null && Starbase.Aggregate.HasStargate; }

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
                if (Player != null && !ContributesOnlyLeftoverToResearch)
                {
                    return (int)(ResourcesPerYear * (1 - Player.ResearchAmount / 100.0) + .5f);
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
                if (Player != null)
                {
                    return GetResourcesPerYearResearch(Player.ResearchAmount);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Get the number of resources this planet produces per year for research for a given player's research amount.
        /// </summary>
        /// <param name="researchAmount"></param>
        /// <returns></returns>
        public int GetResourcesPerYearResearch(int researchAmount)
        {
            if (!ContributesOnlyLeftoverToResearch)
            {
                return (int)(ResourcesPerYear * (researchAmount / 100.0));
            }
            else
            {
                return 0;
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
        /// Get the actual quantity we will build (as opposed to the amount requested by the player)
        /// We can't build more mines than the planet supports
        /// Auto build steps will also only build what is necessary
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetQuantityToBuild(int requestedQuantity, QueueItemType type)
        {
            // if we are autobuilding, don't build more than our max
            // i.e. if we have 95 mines, 100 max mines, and we are autobuilding 10 mines
            // we should only actually autobuild 5 mines
            switch (type)
            {
                case QueueItemType.AutoMines:
                    requestedQuantity = Math.Min(requestedQuantity, MaxMines - Mines);
                    break;
                case QueueItemType.AutoFactories:
                    requestedQuantity = Math.Min(requestedQuantity, MaxFactories - Factories);
                    break;
                case QueueItemType.AutoDefenses:
                    requestedQuantity = Math.Min(requestedQuantity, MaxDefenses - Defenses);
                    break;
                case QueueItemType.Mine:
                    requestedQuantity = Math.Min(requestedQuantity, MaxPossibleMines - Mines);
                    break;
                case QueueItemType.Factory:
                    requestedQuantity = Math.Min(requestedQuantity, MaxPossibleFactories - Factories);
                    break;
                case QueueItemType.Defenses:
                    requestedQuantity = Math.Min(requestedQuantity, MaxDefenses - Defenses);
                    break;
                case QueueItemType.TerraformEnvironment:
                case QueueItemType.AutoMaxTerraform:
                    requestedQuantity = Math.Min(requestedQuantity, GetTerraformAmount().Sum);
                    break;
                case QueueItemType.AutoMinTerraform:
                    requestedQuantity = Math.Min(requestedQuantity, GetMinTerraformAmount().Sum);
                    break;
            }
            return requestedQuantity;
        }

        #region Terraforming

        public bool CanTerraform(Player player = null)
        {
            return GetTerraformAmount(player).Sum > 0;
        }

        /// <summary>
        /// Get the total amount we can terraform this planet
        /// </summary>
        /// <param name="planet">The planet to check for tarraformability</param>
        /// <param name="player">The player to check terraformability for. If not set this function uses the planet owner</param>
        /// <returns></returns>
        public Hab GetTerraformAmount(Player player = null)
        {
            player = player == null ? Player : player;
            Hab terraformAmount = new Hab();
            if (player == null)
            {
                // can't terraform, return an empty Hab
                return terraformAmount;
            }

            var bestTotalTerraform = player.GetBestTerraform(TerraformHabType.All);
            var totalTerraformAbility = bestTotalTerraform == null ? 0 : bestTotalTerraform.Ability;

            foreach (HabType habType in Enum.GetValues(typeof(HabType)))
            {
                // get the two ways we can terraform
                var bestHabTerraform = player.GetBestTerraform((TerraformHabType)habType);

                // find out which terraform tech has the greater terraform ability
                var ability = 0;
                if (bestHabTerraform != null)
                {
                    ability = Mathf.Max(bestHabTerraform.Ability, totalTerraformAbility);
                }

                if (ability == 0)
                {
                    continue;
                }

                // The distance from the starting hab of this planet
                int fromIdealBaseDistance = Math.Abs(player.Race.HabCenter[habType] - BaseHab.Value[habType]);

                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - Hab.Value[habType];
                int fromIdealDistance = Math.Abs(fromIdeal);

                // if we have any left to terraform
                if (fromIdealDistance > 0)
                {
                    // we can either terrform up to our full ability, or however much
                    // we have left to terraform on this
                    var alreadyTerraformed = (fromIdealBaseDistance - fromIdealDistance);
                    terraformAmount = terraformAmount.WithType(habType, Math.Min(ability - alreadyTerraformed, fromIdealDistance));
                }
            }

            return terraformAmount;
        }

        /// <summary>
        /// Get the minimum amount we need to terraform this planet to make it habitable (if we can terraform it at all)
        /// </summary>
        /// <param name="planet">The planet to check for tarraformability</param>
        /// <param name="player">The player to check terraformability for. If not set this function uses the planet owner</param>
        /// <returns></returns>
        public Hab GetMinTerraformAmount(Player player = null)
        {
            player = player == null ? Player : player;
            Hab terraformAmount = new Hab();
            if (player == null || Hab == null)
            {
                // can't terraform, return an empty Hab
                return terraformAmount;
            }

            var bestTotalTerraform = player.GetBestTerraform(TerraformHabType.All);
            var totalTerraformAbility = bestTotalTerraform == null ? 0 : bestTotalTerraform.Ability;

            foreach (HabType habType in Enum.GetValues(typeof(HabType)))
            {
                // get the two ways we can terraform
                var bestHabTerraform = player.GetBestTerraform((TerraformHabType)habType);

                // find out which terraform tech has the greater terraform ability
                var ability = totalTerraformAbility;
                if (bestHabTerraform != null)
                {
                    ability = Mathf.Max(ability, bestHabTerraform.Ability);
                }

                if (ability == 0)
                {
                    continue;
                }

                // The distance from the starting hab of this planet
                int fromIdealBaseDistance = Math.Abs(player.Race.HabCenter[habType] - BaseHab.Value[habType]);
                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - Hab.Value[habType];
                int fromIdealDistance = Math.Abs(fromIdeal);

                // the distance from the current hab of this planet to our minimum hab threshold
                // If this is positive, it means we need to terraform a certain percent to get it in range
                int fromHabitableDistance = 0;
                int planetHabValue = Hab.Value[habType];
                int playerHabIdeal = player.Race.HabCenter[habType];
                if (planetHabValue > playerHabIdeal)
                {
                    // this planet is higher that we want, check the upper bound distance
                    // if the player's high temp is 85, and this planet is 83, we are already in range
                    // and don't need to min-terraform. If the planet is 87, we need to drop it 2 to be in range.
                    fromHabitableDistance = planetHabValue - player.Race.HabHigh[habType];
                }
                else
                {
                    // this planet is lower than we want, check the lower bound distance
                    // if the player's low temp is 15, and this planet is 17, we are already in range
                    // and don't need to min-terraform. If the planet is 13, we need to increase it 2 to be in range.
                    fromHabitableDistance = player.Race.HabLow[habType] - planetHabValue;
                }

                // if we are already in range, set this to 0 because we don't want to terraform anymore
                if (fromHabitableDistance < 0)
                {
                    fromHabitableDistance = 0;
                }

                // if we have any left to terraform
                if (fromHabitableDistance > 0)
                {
                    // we can either terrform up to our full ability, or however much
                    // we have left to terraform on this
                    var alreadyTerraformed = (fromIdealBaseDistance - fromIdealDistance);
                    var terraformAmountPossible = Math.Min(ability - alreadyTerraformed, fromIdealDistance);

                    // if we are in range for this hab type, we won't terraform at all, otherwise return the max possible terraforming
                    // left.
                    terraformAmount = terraformAmount.WithType(habType, Math.Min(fromHabitableDistance, terraformAmountPossible));
                }
            }

            return terraformAmount;
        }

        /// <summary>
        /// Get the best hab to terraform (the one with the most distance away from ideal that we can still terraform)
        /// </summary>
        /// <param name="planet"></param>
        /// <returns></returns>
        public HabType? GetBestTerraform()
        {
            var player = Player;
            if (player == null)
            {
                return null;
            }
            // if we can terraform any, return true
            var largestDistance = 0;
            HabType? bestHabType = null;

            var bestTotalTerraform = player.GetBestTerraform(TerraformHabType.All);
            var totalTerraformAbility = bestTotalTerraform == null ? 0 : bestTotalTerraform.Ability;

            foreach (HabType habType in Enum.GetValues(typeof(HabType)))
            {
                // get the two ways we can terraform
                var bestHabTerraform = player.GetBestTerraform((TerraformHabType)habType);

                // find out which terraform tech has the greater terraform ability
                var ability = totalTerraformAbility;
                if (bestHabTerraform != null)
                {
                    ability = Mathf.Max(ability, bestHabTerraform.Ability);
                }

                if (ability == 0)
                {
                    continue;
                }

                // The distance from the starting hab of this planet
                int fromIdealBaseDistance = Math.Abs(player.Race.HabCenter[habType] - BaseHab.Value[habType]);

                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - Hab.Value[habType];
                int fromIdealDistance = Math.Abs(fromIdeal);

                int terraformAmount = 0;
                // if we have any left to terraform
                if (fromIdealDistance > 0)
                {
                    // we can either terrform up to our full ability, or however much
                    // we have left to terraform on this
                    var alreadyTerraformed = (fromIdealBaseDistance - fromIdealDistance);
                    terraformAmount = Math.Min(ability - alreadyTerraformed, fromIdealDistance);
                }

                // we want to get the largest distance that we can terraform
                if (fromIdealDistance > largestDistance && terraformAmount > 0)
                {
                    largestDistance = fromIdealDistance;
                    bestHabType = habType;
                }
            }

            return bestHabType;
        }

        #endregion

        /// <summary>
        /// Attempt to transfer cargo to/from this planet
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns>true if we have minerals we can transfer</returns>
        public bool AttemptTransfer(Cargo transfer, int fuel = 0)
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