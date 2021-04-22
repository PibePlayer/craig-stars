using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// A single step in turn generation, like moving fleets, bombing planets, etc
    /// </summary>
    public abstract class Step
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Step));

        public Game Game { get; private set; }
        public TurnGeneratorState State { get; private set; }
        public TurnGenerationContext Context { get; private set; }

        public List<Planet> OwnedPlanets { get; private set; }

        Stopwatch stopwatch = new Stopwatch();

        protected Step(Game game, TurnGeneratorState state)
        {
            Game = game;
            State = state;
        }

        /// <summary>
        /// Execute this step
        /// </summary>
        public void Execute(TurnGenerationContext context, List<Planet> ownedPlanets)
        {
            stopwatch.Start();
            log.Debug($"{Game.Year}: Beginning {this.GetType().ToString()}");
            Context = context;

            PreProcess(ownedPlanets);
            Process();
            PostProcess();

            stopwatch.Stop();
            log.Debug($"{Game.Year}: Completed {this.GetType().ToString()} ({stopwatch.ElapsedMilliseconds}ms)");
            // log.Debug($"{Game.Year}: Completed {this.GetType().ToString()})");
        }

        /// <summary>
        /// Override for any pre-processing
        /// </summary>
        /// <param name="ownedPlanets">A subset of planets owned by players</param>
        public virtual void PreProcess(List<Planet> ownedPlanets)
        {
            OwnedPlanets = ownedPlanets;
        }

        /// <summary>
        /// Do whatever processing this step requires for turn generation
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Override for any post-processing
        /// </summary>
        public virtual void PostProcess() { }

    }
}