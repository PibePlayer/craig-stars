using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class MysteryTraderMoveStep : TurnGenerationStep
    {
        public MysteryTraderMoveStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.MysteryTraderMoveStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}