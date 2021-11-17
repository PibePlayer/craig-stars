using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class RandomMineralDiscoveryStep : TurnGenerationStep
    {
        public RandomMineralDiscoveryStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.RandomMineralDiscoveryStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}