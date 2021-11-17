using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class RandomPlanetaryChangeStep : TurnGenerationStep
    {
        public RandomPlanetaryChangeStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.RandomPlanetaryChangeStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}