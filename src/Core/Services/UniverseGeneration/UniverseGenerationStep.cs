using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// A single step in universe generation
    /// </summary>
    public abstract class UniverseGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(UniverseGenerationStep));

        protected readonly IProvider<Game> gameProvider;

        protected Game Game { get => gameProvider.Item; }
        public UniverseGenerationState State { get; private set; }
        protected Dictionary<string, object> Context { get; set; } = new();

        Stopwatch stopwatch = new Stopwatch();

        protected UniverseGenerationStep(IProvider<Game> gameProvider, UniverseGenerationState state)
        {
            this.gameProvider = gameProvider;
            State = state;
        }

        /// <summary>
        /// Execute this step
        /// </summary>
        public void Execute(Dictionary<string, object> context)
        {
            stopwatch.Start();
            log.Debug($"{Game.Name}: Beginning {this.GetType().ToString()}");
            Context = context;

            Process();

            stopwatch.Stop();
            log.Debug($"{Game.Name}: Completed {this.GetType().ToString()} ({stopwatch.ElapsedMilliseconds}ms)");
        }

        /// <summary>
        /// Do whatever processing this step requires for universe generation
        /// </summary>
        public abstract void Process();

    }
}