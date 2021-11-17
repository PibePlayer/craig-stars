using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class FleetWaypoint1Step : AbstractFleetWaypointStep
    {
        public FleetWaypoint1Step(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            PlanetService planetService,
            InvasionProcessor invasionProcessor,
            PlanetDiscoverer planetDiscoverer
        ) : base(gameProvider, rulesProvider, planetService, invasionProcessor, planetDiscoverer, TurnGenerationState.FleetWaypoint1Step, 1) { }
    }
}