using System;
using CraigStars.Singletons;

namespace CraigStars
{
    /// <summary>
    /// Turn processors are used by the UI (and optionally by players) to
    /// process new turn data
    /// </summary>
    public abstract class TurnProcessor
    {
        public Player Player { get; set; }

        public TurnProcessor(Player player)
        {
            Player = player;
        }

        public void Subscribe()
        {
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public void Unsubscribe()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnTurnPassed(int year)
        {
            Process(year);
        }

        /// <summary>
        /// Process a turn
        /// </summary>
        protected abstract void Process(int year);
    }
}