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
        private static readonly ILog log = LogManager.GetLogger(typeof(PlanetProductionTurnProcessor));

        // the required population density required of a planet in order to suck people off of it
        // setting this to .33 because we don't want to suck people off a planet until it's reached the
        // max of its growth rate (over 1/3rd crowded)
        private const float PopulationDensityRequired = .33f;

        public PlanetProductionTurnProcessor(PublicGameInfo gameInfo) : base(gameInfo) { }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(Player player)
        {
            foreach (var planet in player.Planets)
            {
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoMine, 25), 1);
                planet.ProductionQueue.EnsureHasItem(new ProductionQueueItem(QueueItemType.AutoFactory, 25), 2);
            }
        }

    }
}