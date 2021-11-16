using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypoint0Step : AbstractFleetWaypointStep
    {
        public FleetWaypoint0Step(
                IProvider<Game> gameProvider,
                IRulesProvider rulesProvider,
                PlanetService planetService,
                InvasionProcessor invasionProcessor,
                PlanetDiscoverer planetDiscoverer
            ) : base(gameProvider, rulesProvider, planetService, invasionProcessor, planetDiscoverer, 0) { }
    }
}