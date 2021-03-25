using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Grow population on planets
    /// </summary>
    public class PlanetGrowStep : Step
    {
        public PlanetGrowStep(Game game, TurnGeneratorState state) : base(game, state) { }

        public override void Process()
        {
            OwnedPlanets.ForEach(p => p.Population += p.GrowthAmount);
        }

    }
}