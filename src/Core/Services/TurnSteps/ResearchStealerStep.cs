using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class ResearchStealerStep : TurnGenerationStep
    {
        public ResearchStealerStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.ResearchStealerStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}