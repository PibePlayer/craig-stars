using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigStars
{
    /// <summary>
    /// Submit turns for each AI player
    /// </summary>
    public class AITurnSubmitter
    {
        static CSLog log = LogProvider.GetLogger(typeof(AITurnSubmitter));

        public event Action<PlayerOrders> TurnSubmitRequestedEvent;

        ITurnProcessorManager turnProcessorManager;

        public AITurnSubmitter(ITurnProcessorManager turnProcessorManager)
        {
            this.turnProcessorManager = turnProcessorManager;
        }

        /// <summary>
        /// Submit any AI turns
        /// This submits all turns in separate threads and returns a Task for them all to complete
        /// </summary>
        public void SubmitAITurns(Game game)
        {
            log.Debug($"Submitting AI turns");
            var aiPlayers = game.Players.Where(player => player.AIControlled && !player.SubmittedTurn).ToList();
            Task.Run(() =>
            {
                try
                {
                    foreach (var player in aiPlayers)
                    {
                        try
                        {
                            foreach (var processor in turnProcessorManager.TurnProcessors)
                            {
                                processor.Process(game.GameInfo, player);
                            }
                            // We are done processing, submit a turn
                            TurnSubmitRequestedEvent?.Invoke(player.GetOrders());
                        }
                        catch (Exception e)
                        {
                            log.Error($"Failed to submit AI turn {player}", e);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Failed to submit AI turns", e);
                }
            }).Wait();
        }
    }
}
