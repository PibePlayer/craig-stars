using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class RandomCometStrikeStep : TurnGenerationStep
    {
        public RandomCometStrikeStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.RandomCometStrikeStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}