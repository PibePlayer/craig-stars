using System;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Game logic methods for planets
    /// </summary>
    public class PlanetService
    {
        public int GetMaxMines(Planet planet, Player player) => planet.Population * player.Race.NumMines / 10000;
        public int GetMaxPossibleMines(Planet planet, Player player) => GetMaxPopulation(planet, player, player.Rules) * player.Race.NumMines / 10000;
        public int GetMaxFactories(Planet planet, Player player) => planet.Population * player.Race.NumFactories / 10000;
        public int GetMaxPossibleFactories(Planet planet, Player player) => GetMaxPopulation(planet, player, player.Rules) * player.Race.NumFactories / 10000;
        public int GetMaxDefenses(Planet planet, Player player) => 100;

        # region Population & Growth

        public float GetPopulationDensity(Planet planet, Player player, Rules rules) => GetPopulationDensity(planet, player, rules, planet.Population);
        public float GetPopulationDensity(Planet planet, Player player, Rules rules, int population) => population > 0 ? (float)population / GetMaxPopulation(planet, player, rules) : 0;

        /// <summary>
        /// The max population for the race on this planet
        /// TODO: support this later
        /// /// </summary>
        /// <returns></returns>
        public int GetMaxPopulation(Planet planet, Player player, Rules rules)
        {
            var race = player.Race;
            if (planet.Hab is Hab planetHab)
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
        /// The amount the population for this planet will grow next turn
        /// </summary>
        /// <returns></returns>
        public int GetGrowthAmount(Planet planet, Player player, Rules rules)
        {
            var race = player.Race;
            if (planet.Hab is Hab hab)
            {
                double capacity = ((double)planet.Population / GetMaxPopulation(planet, player, rules));
                var habValue = race.GetPlanetHabitability(hab);
                if (habValue > 0)
                {
                    int popGrowth = (int)(planet.Population * (race.GrowthRate / 100.0) * (habValue / 100.0));

                    if (capacity > .25)
                    {
                        double crowdingFactor = 16.0 / 9.0 * (1.0 - capacity) * (1.0 - capacity);
                        popGrowth = (int)((double)(popGrowth) * crowdingFactor);
                    }

                    // round to the nearest 100 colonists
                    return RoundToNearest(popGrowth);
                }
                else
                {
                    // kill off (habValue / 10)% colonists every year. I.e. a habValue of -4% kills off .4%
                    int deathAmount = (int)(planet.Population * (habValue / 1000.0));
                    return RoundToNearest(Mathf.Clamp(deathAmount, deathAmount, -100));
                }

            }
            return 0;
        }

        #endregion

        #region Minerals

        /// <summary>
        /// The mineral output of this planet if it is owned
        /// </summary>
        /// <returns></returns>
        public Mineral GetMineralOutput(Planet planet, Player player) => GetMineralOutput(planet, planet.Mines, player.Race.MineOutput);

        /// <summary>
        /// Get the amount of minerals this planet outputs for a a certain number of mines, given a race mineOutput
        /// </summary>
        /// <param name="mineOutput"></param>
        /// <returns></returns>
        public Mineral GetMineralOutput(Planet planet, int numMines, int mineOutput = 10)
        {
            return new Mineral(
                MineralsPerYear(planet.MineralConcentration.Ironium, numMines, mineOutput),
                MineralsPerYear(planet.MineralConcentration.Boranium, numMines, mineOutput),
                MineralsPerYear(planet.MineralConcentration.Germanium, numMines, mineOutput)
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

        #endregion

        #region Resources

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet generates in a year</value>
        public int GetResourcesPerYear(Planet planet, Player player)
        {
            var race = player.Race;

            // compute resources from population
            int resourcesFromPop = planet.Population == 0 ? 0 : planet.Population / race.ColonistsPerResource;

            // compute resources from factories
            int resourcesFromFactories = planet.Factories * race.FactoryOutput / 10;

            // return the sum
            return resourcesFromPop + resourcesFromFactories;
        }

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet will contribute per year</value>
        public int GetResourcesPerYearAvailable(Planet planet, Player player) => planet.ContributesOnlyLeftoverToResearch ? 
            GetResourcesPerYear(planet, player) 
            : (int)(GetResourcesPerYear(planet, player) * (1 - player.ResearchAmount / 100.0) + .5f);

        /// <summary>
        /// Determine the number of resources this planet generates in a year
        /// </summary>
        /// <value>The number of resources this planet will contribute per year</value>
        public int GetResourcesPerYearResearch(Planet planet, Player player) => GetResourcesPerYearResearch(planet, player, player.ResearchAmount);

        /// <summary>
        /// Get the number of resources this planet produces per year for research for a given player's research amount.
        /// </summary>
        /// <param name="researchAmount"></param>
        /// <returns></returns>
        public int GetResourcesPerYearResearch(Planet planet, Player player, int researchAmount) => planet.ContributesOnlyLeftoverToResearch ? 0 : (int)(GetResourcesPerYear(planet, player) * (researchAmount / 100.0));

        #endregion

        #region Defense


        /// <summary>
        /// Get the defense coverage for this planet, assuming it has a player
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverage(Planet planet, Player player) => GetDefenseCoverage(planet, player, player.GetBestDefense());

        /// <summary>
        /// Get the defense coverage for this planet for a given defense type
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverage(Planet planet, Player player, TechDefense defense)
        {
            if (planet.Defenses > 0 && defense != null)
            {
                return (float)(1.0 - (Math.Pow((1 - (defense.DefenseCoverage / 100)), Mathf.Clamp(planet.Defenses, 0, GetMaxDefenses(planet, player)))));
            }
            return 0;
        }

        /// <summary>
        /// Get the defense coverage for this planet against smart bombs, assuming it has a player
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverageSmart(Planet planet, Player player, Rules rules) => GetDefenseCoverageSmart(planet, player, rules, player.GetBestDefense());

        /// <summary>
        /// Get the defense coverage for this planet for a given defense type
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public float GetDefenseCoverageSmart(Planet planet, Player player, Rules rules, TechDefense defense)
        {
            if (planet.Defenses > 0 && defense != null)
            {
                return (float)(1.0 - (Math.Pow((1 - (defense.DefenseCoverage / 100 * rules.SmartDefenseCoverageFactor)), Mathf.Clamp(planet.Defenses, 0, GetMaxDefenses(planet, player)))));
            }
            return 0;
        }

        #endregion

        #region Production

        /// <summary>
        /// Return true if this is a planet that the current player can build
        /// </summary>
        /// <param name="mass"></param>
        /// <returns></returns>
        public bool CanBuild(Planet planet, Player player, int mass)
        {
            if (planet.Player == player && planet.HasStarbase)
            {
                var dockCapacity = planet.Starbase.DockCapacity;
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
        public int GetQuantityToBuild(Planet planet, Player player, int requestedQuantity, QueueItemType type)
        {
            // if we are autobuilding, don't build more than our max
            // i.e. if we have 95 mines, 100 max mines, and we are autobuilding 10 mines
            // we should only actually autobuild 5 mines
            switch (type)
            {
                case QueueItemType.AutoMines:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxMines(planet, player) - planet.Mines);
                    break;
                case QueueItemType.AutoFactories:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxFactories(planet, player) - planet.Factories);
                    break;
                case QueueItemType.AutoDefenses:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxDefenses(planet, player) - planet.Defenses);
                    break;
                case QueueItemType.Mine:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxPossibleMines(planet, player) - planet.Mines);
                    break;
                case QueueItemType.Factory:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxPossibleFactories(planet, player) - planet.Factories);
                    break;
                case QueueItemType.Defenses:
                    requestedQuantity = Math.Min(requestedQuantity, GetMaxDefenses(planet, player) - planet.Defenses);
                    break;
                case QueueItemType.TerraformEnvironment:
                case QueueItemType.AutoMaxTerraform:
                    requestedQuantity = Math.Min(requestedQuantity, GetTerraformAmount(planet, player).AbsSum);
                    break;
                case QueueItemType.AutoMinTerraform:
                    requestedQuantity = Math.Min(requestedQuantity, GetMinTerraformAmount(planet, player).AbsSum);
                    break;
            }
            return requestedQuantity;
        }

        #endregion

        #region Terraforming

        public bool CanTerraform(Planet planet, Player player)
        {
            return GetTerraformAmount(planet, player).AbsSum > 0;
        }

        /// <summary>
        /// Get the total amount we can terraform this planet
        /// </summary>
        /// <param name="planet">The planet to check for tarraformability</param>
        /// <param name="player">The player to check terraformability for. If not set this function uses the planet owner</param>
        /// <returns></returns>
        public Hab GetTerraformAmount(Planet planet, Player player)
        {
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
                int fromIdealBase = player.Race.HabCenter[habType] - planet.BaseHab.Value[habType];

                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - planet.Hab.Value[habType];

                // if we have any left to terraform
                if (fromIdeal > 0)
                {
                    // i.e. our ideal is 50 and the planet hab is 47

                    // we can either terrform up to our full ability, or however much
                    // we have left to terraform on this
                    var alreadyTerraformed = (fromIdealBase - fromIdeal);
                    terraformAmount = terraformAmount.WithType(habType, Math.Min(ability - alreadyTerraformed, fromIdeal));
                }
                else if (fromIdeal < 0)
                {
                    // i.e. our ideal is 50 and the planet hab is 53
                    var alreadyTerraformed = (fromIdeal - fromIdealBase);
                    terraformAmount = terraformAmount.WithType(habType, Math.Max(-(ability - alreadyTerraformed), fromIdeal));
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
        public Hab GetMinTerraformAmount(Planet planet, Player player)
        {
            Hab terraformAmount = new Hab();
            if (player == null || planet.Hab == null)
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
                int fromIdealBaseDistance = Math.Abs(player.Race.HabCenter[habType] - planet.BaseHab.Value[habType]);
                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - planet.Hab.Value[habType];
                int fromIdealDistance = Math.Abs(fromIdeal);

                // the distance from the current hab of this planet to our minimum hab threshold
                // If this is positive, it means we need to terraform a certain percent to get it in range
                int fromHabitableDistance = 0;
                int planetHabValue = planet.Hab.Value[habType];
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
        public HabType? GetBestTerraform(Planet planet, Player player)
        {
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
                int fromIdealBaseDistance = Math.Abs(player.Race.HabCenter[habType] - planet.BaseHab.Value[habType]);

                // the distance from the current hab of this planet
                int fromIdeal = player.Race.HabCenter[habType] - planet.Hab.Value[habType];
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
    }
}