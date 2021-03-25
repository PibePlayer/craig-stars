using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Bombers orbiting enemy planets will Bomb planets
    /// </summary>
    public class PlanetBombStep : Step
    {
        public PlanetBombStep(Game game, TurnGeneratorState state) : base(game, state) { }

        PlanetBomber planetBomber = new PlanetBomber();

        public override void Process()
        {
            OwnedPlanets.ForEach(p =>
            {
                planetBomber.BombPlanet(p);
                if (p.Population == 0)
                {
                    EventManager.PublishPlanetPopulationEmptiedEvent(p);
                }
            });
        }

    }
}