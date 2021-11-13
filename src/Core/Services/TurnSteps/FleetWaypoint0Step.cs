using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypoint0Step : AbstractFleetWaypointStep
    {
        public FleetWaypoint0Step(
                IProvider<Game> gameProvider,
                PlanetService planetService,
                InvasionProcessor invasionProcessor,
                PlanetDiscoverer planetDiscoverer
            ) : base(gameProvider, planetService, invasionProcessor, planetDiscoverer, 0) { }
    }
}