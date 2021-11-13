using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// A single step in turn generation, like moving fleets, bombing planets, etc
    /// </summary>
    public abstract class TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnGenerationStep));

        protected readonly IProvider<Game> gameProvider;

        protected Game Game { get => gameProvider.Item; }
        public TurnGenerationState State { get; }
        protected TurnGenerationContext Context { get; private set; }
        public List<Planet> OwnedPlanets { get; private set; }

        Stopwatch stopwatch = new Stopwatch();

        protected TurnGenerationStep(IProvider<Game> gameProvider, TurnGenerationState state)
        {
            this.gameProvider = gameProvider;
            State = state;
        }

        /// <summary>
        /// Execute this step
        /// </summary>
        public void Execute(TurnGenerationContext context, List<Planet> ownedPlanets)
        {
            stopwatch.Reset();
            stopwatch.Start();
            log.Debug($"{Game.Year}: Beginning {this.GetType().ToString()}");
            Context = context;

            PreProcess(ownedPlanets);
            Process();
            PostProcess();

            stopwatch.Stop();

            log.Debug($"{Game.Year}: Completed {this.GetType().ToString()} ({TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds):c}ms)");
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