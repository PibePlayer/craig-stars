using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Turn processors are used by the UI (and optionally by players) to
    /// process new turn data
    /// </summary>
    public abstract class TurnProcessor
    {
        public PublicGameInfo GameInfo { get; set; }
        public TurnProcessor(PublicGameInfo gameInfo)
        {
            GameInfo = gameInfo;
        }

        protected Planet ClosestPlanet(Fleet fleet, List<Planet> unknownPlanets)
        {
            Planet closest = null;
            // find the nearest planet to this fleet
            float dist = 0;
            foreach (Planet planet in unknownPlanets)
            {
                if (closest == null)
                {
                    closest = planet;
                    dist = planet.Position.DistanceSquaredTo(fleet.Position);
                }
                else
                {
                    // figure out the nearest planet to this fleet
                    float new_dist = planet.Position.DistanceSquaredTo(fleet.Position);
                    if (new_dist < dist)
                    {
                        // this planet is closer, save it
                        // and recompute the distance
                        dist = new_dist;
                        closest = planet;
                    }
                }
            }
            return closest;
        }

        /// <summary>
        /// Process a turn
        /// </summary>
        public abstract void Process(Player player);

    }
}