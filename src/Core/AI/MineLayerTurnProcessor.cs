using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

namespace CraigStars
{
    /// <summary>
    /// Make MineLayers lay mines
    /// </summary>
    public class MineLayerTurnProcessor : TurnProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MineLayerTurnProcessor));

        public MineLayerTurnProcessor(PublicGameInfo gameInfo) : base(gameInfo) { }

        /// <summary>
        /// Make sure our mine layers lay mines
        /// </summary>
        public override void Process(Player player)
        {
            foreach (Fleet fleet in player.Fleets.Where(fleet => fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.SpeedMineLayer) ||
            fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.DamageMineLayer)))
            {
                fleet.Waypoints[0].Task = WaypointTask.LayMineField;
            }
        }
    }
}