using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigStars.Server
{
    /// <summary>
    /// Submit turns for each AI player
    /// </summary>
    public class AITurnSubmitter
    {
        static CSLog log = LogProvider.GetLogger(typeof(AITurnSubmitter));

        public event Action<Player> TurnSubmitRequestedEvent;

        ITurnProcessorManager turnProcessorManager;
        bool multithreaded;

        public AITurnSubmitter(ITurnProcessorManager turnProcessorManager, bool multithreaded)
        {
            this.turnProcessorManager = turnProcessorManager;
            this.multithreaded = multithreaded;
        }

        /// <summary>
        /// Submit any AI turns
        /// This submits all turns in separate threads and returns a Task for them all to complete
        /// </summary>
        public void SubmitAITurns(Game game)
        {
            log.Debug($"Submitting AI turns");
            Task.Run(() =>
            {
                foreach (var player in game.Players.Where(player => player.AIControlled && !player.SubmittedTurn))
                {
                    try
                    {
                        foreach (var processor in turnProcessorManager.TurnProcessors)
                        {
                            processor.Process(player);
                        }
                        // We are done processing, submit a turn
                        TurnSubmitRequestedEvent?.Invoke(player);
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to submit AI turn {player}", e);
                    }
                }
            });
            // var tasks = new List<Action>();
            // // submit AI turns
            // foreach (var player in game.Players)
            // {
            //     if (player.AIControlled && !player.SubmittedTurn)
            //     {
            //         Action submitAITurn = () =>
            //         {
            //             try
            //             {
            //                 foreach (var processor in turnProcessorManager.TurnProcessors)
            //                 {
            //                     processor.Process(player);
            //                 }
            //                 // We are done processing, submit a turn
            //                 TurnSubmitRequestedEvent?.Invoke(player);
            //             }
            //             catch (Exception e)
            //             {
            //                 log.Error($"Failed to submit AI turn {player}", e);
            //             }
            //         };
            //         tasks.Add(submitAITurn);
            //     }
            // }
            // if (multithreaded)
            // {
            //     Task.Run(() => Task.WaitAll(tasks.Select(task => Task.Run(task)).ToArray()));
            // }
            // else
            // {
            //     tasks.ForEach(_ => _.Invoke());
            // }
        }
    }
}
