using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigStars.Client
{
    /// <summary>
    /// This component is used for 
    /// </summary>
    public abstract class PlanetProductionQueueItems : ProductionQueueItems
    {
        protected PlanetService planetService = new();

        public Planet Planet
        {
            get => planet;
            set
            {
                planet = value;
                // update the items when the planet changes
                OnPlanetUpdated();
            }
        }
        Planet planet;

        public bool ContributesOnlyLeftoverToResearch { get; set; }

        protected Cost availableCost = Cost.Zero;
        protected Cost yearlyAvailableCost = Cost.Zero;

        /// <summary>
        /// When a planet is updated, reset our internal Items list to whatever is currently in the planet queue
        /// </summary>
        protected virtual void OnPlanetUpdated()
        {
            Items.Clear();
            if (Planet != null)
            {
                ComputeAvailableResources();
            }
        }

        /// <summary>
        /// Get the color for this ProductionQueueItem based on how many years it takes to build
        /// </summary>
        /// <param name="yearsToBuildAll"></param>
        /// <param name="skipped">true if this item will be skipped (i.e. an auto item that is already at max usable items)</param>
        /// <returns></returns>
        public Color GetColor(int yearsToBuildOne, int yearsToBuildAll, bool skipped)
        {
            if (skipped)
            {
                return GUIColors.ProductionQueueSkippedColor;
            }
            if (yearsToBuildAll <= 1)
            {
                // if we can build them all in one year, color it gree
                return GUIColors.ProductionQueueItemOneYearColor;
            }
            else if (yearsToBuildOne <= 1)
            {
                // if we can build at least one in a year, color it blue
                return GUIColors.ProductionQueueMoreThanOneYearColor;
            }
            else if (yearsToBuildOne >= 100)
            {
                // if it will take more than 100 years to build them all, color it red
                return GUIColors.ProductionQueueNeverBuildColor;
            }

            return Colors.White;
        }

        /// <summary>
        /// Based on the planet, compute how many resources we have available
        /// as well as minerals for building.
        /// </summary>
        protected void ComputeAvailableResources()
        {
            if (Planet != null)
            {
                // figure out how many resources we have per year
                int yearlyResources = 0;
                if (ContributesOnlyLeftoverToResearch)
                {
                    yearlyResources = planetService.GetResourcesPerYear(Planet, Me);
                }
                else
                {
                    yearlyResources = planetService.GetResourcesPerYearAvailable(Planet, Me);
                }

                // this is how man resources and minerals our planet produces each year
                yearlyAvailableCost = new Cost(planetService.GetMineralOutput(Planet, Me), yearlyResources);

                // Get the total availble cost of this planet's yearly output + resources on hand.
                availableCost = yearlyAvailableCost + Planet.Cargo.ToCost();
            }
            else
            {
                yearlyAvailableCost = Cost.Zero;
                availableCost = Cost.Zero;
            }
        }


    }
}