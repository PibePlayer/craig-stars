using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface ITurnProcessorManager
    {
        /// <summary>
        /// Register a TurnProcessor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        void RegisterTurnProcessor(TurnProcessor processor);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TurnProcessor GetTurnProcessor(string name);

        /// <summary>
        /// Get a list of all registered turn processors
        /// </summary>
        IEnumerable<TurnProcessor> TurnProcessors { get; }
    }
}