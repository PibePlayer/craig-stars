using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class MysteryTraderMeetStep : TurnGenerationStep
    {
        public MysteryTraderMeetStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.MysteryTraderMeetStep)
        {
        }

        public override void Process()
        {
            // TODO: fill in
        }
    }
}