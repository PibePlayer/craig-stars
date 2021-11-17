using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetScrapStep : TurnGenerationStep
    {
        public FleetScrapStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetScrapStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}