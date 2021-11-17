using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetRefuelStep : TurnGenerationStep
    {
        public FleetRefuelStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetRefuelStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}