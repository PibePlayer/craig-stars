using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars
{
    /// <summary>
    /// Runs the turn processors for a player
    /// </summary>
    public class TurnProcessorRunner
    {

        /// <summary>
        /// Run all configured TurnProcessors for this player
        /// </summary>
        /// <param name="turnProcessorManager"></param>
        public void RunTurnProcessors(PublicGameInfo gameInfo, Player player, ITurnProcessorManager turnProcessorManager)
        {
            player.Settings.TurnProcessors.ForEach(processorName =>
            {
                var processor = turnProcessorManager.GetTurnProcessor(processorName);
                if (processor != null)
                {
                    processor.Process(gameInfo, player);
                }
            });
        }

    }
}
