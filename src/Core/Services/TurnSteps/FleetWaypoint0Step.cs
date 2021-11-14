using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypoint0Step : AbstractFleetWaypointStep
    {
        public FleetWaypoint0Step(
                IProvider<Game> gameProvider,
                PlayerService playerService,
                PlanetService planetService,
                InvasionProcessor invasionProcessor,
                PlanetDiscoverer planetDiscoverer
            ) : base(gameProvider, playerService, planetService, invasionProcessor, planetDiscoverer, 0) { }
    }
}