using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetTransferStep : TurnGenerationStep
    {
        public FleetTransferStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetTransferStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}