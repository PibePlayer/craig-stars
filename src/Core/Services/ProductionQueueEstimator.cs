using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Service for estimating production queues
    /// </summary>
    public class ProductionQueueEstimator
    {
        PlanetService planetService = new();

        /// <summary>
        /// Completion estimate internal structure
        /// </summary>
        internal readonly struct CompletionEstimate
        {
            public readonly int yearsToBuildOne;
            public readonly int yearsToBuildAll;
            public readonly float percentComplete;
            public CompletionEstimate(int yearsToBuildOne, int yearsToBuildAll, float percentComplete)
            {
                this.yearsToBuildOne = yearsToBuildOne;
                this.yearsToBuildAll = yearsToBuildAll;
                this.percentComplete = percentComplete;
            }
        }

        /// <summary>
        /// For a planet and a set of ProductionQueueItems, calculate the yearsToBuild and yearsToBuildOne properties
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="items"></param>
        /// <param name="contributesOnlyLeftoverToResearch"></param>
        public void CalculateCompletionEstimates(Planet planet, Player player, List<ProductionQueueItem> items, bool contributesOnlyLeftoverToResearch)
        {
            // figure out how many resources we have per year
            int yearlyResources = 0;
            if (contributesOnlyLeftoverToResearch)
            {
                yearlyResources = planetService.GetResourcesPerYear(planet, player);
            }
            else
            {
                yearlyResources = planetService.GetResourcesPerYearAvailable(planet, player);
            }

            // this is how man resources and minerals our planet produces each year
            Cost yearlyAvailableToSpend = new Cost(planetService.GetMineralOutput(planet, player), yearlyResources);

            // allocate all minerals on the planet to our starting "cost"
            Cost previousItemsCost = -planet.Cargo.ToCost();

            // go through each item and update it's YearsToComplete field
            foreach (var item in items)
            {
                previousItemsCost = previousItemsCost - item.Allocated;
                // figure out how much this item costs
                var costOfOne = item.GetCostOfOne(planet.Player);
                if (item.Type == QueueItemType.Starbase && planet.HasStarbase)
                {
                    costOfOne = planet.Starbase.GetUpgradeCost(item.Design);
                }

                var estimate = GetCompletionEstimate(yearlyAvailableToSpend, costOfOne, previousItemsCost, item);
                item.yearsToBuildAll = estimate.yearsToBuildAll;
                item.yearsToBuildOne = estimate.yearsToBuildOne;
                item.percentComplete = estimate.percentComplete;

                // increase our previousItemsCost for the next item
                // reduce the available resources for next estimate
                previousItemsCost += costOfOne * item.Quantity;

            }
        }

        /// <summary>
        /// Get the estimated number of years to complete this item, based on it's location in the queue
        /// and the costs of all items before it
        /// </summary>
        /// <param name="costOfOne">The cost of this item</param>
        internal CompletionEstimate GetCompletionEstimate(Cost yearlyAvailableToSpend, Cost costOfOne, Cost previousItemsCost, ProductionQueueItem item)
        {
            // Get the total cost of this item plus any previous items in the queue
            // and subtract what we have on hand (that will be applied this year)
            Cost costOfAll = (costOfOne * item.Quantity);
            Cost totalCostToBuildOne = costOfOne + previousItemsCost;
            Cost totalCostToBuildAll = costOfAll + previousItemsCost;

            // If we have a bunch of leftover minerals because our planet is full, 0 those out
            totalCostToBuildOne = new Cost(
                Math.Max(0, totalCostToBuildOne.Ironium),
                Math.Max(0, totalCostToBuildOne.Boranium),
                Math.Max(0, totalCostToBuildOne.Germanium),
                Math.Max(0, totalCostToBuildOne.Resources)
            );
            totalCostToBuildAll = new Cost(
                Math.Max(0, totalCostToBuildAll.Ironium),
                Math.Max(0, totalCostToBuildAll.Boranium),
                Math.Max(0, totalCostToBuildAll.Germanium),
                Math.Max(0, totalCostToBuildAll.Resources)
            );

            float percentComplete = item.Allocated != Cost.Zero ? Mathf.Clamp(item.Allocated / costOfAll, 0, 1) : 0;
            int yearsToBuildAll = totalCostToBuildAll == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(1f / (yearlyAvailableToSpend / totalCostToBuildAll), 0, int.MaxValue));
            int yearsToBuildOne = totalCostToBuildOne == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(1f / (yearlyAvailableToSpend / totalCostToBuildOne), 0, int.MaxValue));
            return new CompletionEstimate(yearsToBuildOne, yearsToBuildAll, percentComplete);
        }
    }
}