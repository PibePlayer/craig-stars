using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy scout ships
    /// </summary>
    public class PlanetProductionTurnProcessor : TurnProcessor
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetProductionTurnProcessor));
        private readonly PlanetService planetService;
        private readonly ProductionQueueEstimator estimator;

        // the required population density required of a planet in order to suck people off of it
        // setting this to .33 because we don't want to suck people off a planet until it's reached the
        // max of its growth rate (over 1/3rd crowded)
        private const float PopulationDensityRequired = .33f;

        public PlanetProductionTurnProcessor(PlanetService planetService, ProductionQueueEstimator estimator) : base("Planet Production Manager")
        {
            this.planetService = planetService;
            this.estimator = estimator;
        }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            foreach (var planet in player.Planets)
            {
                if (planetService.GetBestTerraform(planet, player) != null)
                {
                    planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5), 1);
                }
                else
                {
                    planet.ProductionQueue.RemoveItem(QueueItemType.AutoMinTerraform);
                    planet.ProductionQueue.RemoveItem(QueueItemType.AutoMaxTerraform);
                    planet.ProductionQueue.RemoveItem(QueueItemType.TerraformEnvironment);
                }
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoFactories, 10), 2);
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoMines, 10), 3);

                // put some larger production at the end
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoFactories, 50));
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoMines, 50));
            }
        }

    }
}