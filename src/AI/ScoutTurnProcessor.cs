using System;
using CraigStars.Singletons;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy scout ships
    /// </summary>
    public class ScoutTurnProcessor : TurnProcessor
    {

        public ScoutTurnProcessor(Player player) : base(player)
        {

        }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        protected override void Process(int year)
        {

        }
    }
}