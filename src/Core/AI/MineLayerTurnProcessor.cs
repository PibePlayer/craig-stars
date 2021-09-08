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
        static CSLog log = LogProvider.GetLogger(typeof(MineLayerTurnProcessor));

        public MineLayerTurnProcessor() : base("Mine Layer") { }

        /// <summary>
        /// Make sure our mine layers lay mines
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            foreach (Fleet fleet in player.Fleets.Where(fleet => fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.SpeedMineLayer) ||
            fleet.Aggregate.Purposes.Contains(ShipDesignPurpose.DamageMineLayer)))
            {
                fleet.Waypoints[0].Task = WaypointTask.LayMineField;
            }
        }
    }
}