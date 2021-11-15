using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypoint1Step : AbstractFleetWaypointStep
    {
        public FleetWaypoint1Step(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            PlayerService playerService,
            PlanetService planetService,
            InvasionProcessor invasionProcessor,
            PlanetDiscoverer planetDiscoverer
        ) : base(gameProvider, rulesProvider, playerService, planetService, invasionProcessor, planetDiscoverer, 1) { }
    }
}