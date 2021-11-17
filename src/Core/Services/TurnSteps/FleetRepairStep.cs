using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetRepairStep : TurnGenerationStep
    {
        public FleetRepairStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetRepairStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}