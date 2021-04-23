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
        private static readonly ILog log = LogManager.GetLogger(typeof(UniverseGenerationStep));

        public Game Game { get; private set; }
        public UniverseGenerationState State { get; private set; }
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

        public List<Planet> OwnedPlanets { get; private set; }

        Stopwatch stopwatch = new Stopwatch();

        protected UniverseGenerationStep(Game game, UniverseGenerationState state)
        {
            Game = game;
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