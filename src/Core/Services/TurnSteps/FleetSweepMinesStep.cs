using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;
using log4net;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class FleetSweepMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));

        public FleetSweepMinesStep(Game game) : base(game, TurnGenerationState.MineSweeping) { }

        public override void Process()
        {
            // Separate our waypoint tasks into groups
            foreach (var fleet in Game.Fleets.Where(fleet => fleet.Aggregate.MineSweep > 0))
            {
                // sweep mines from any minefields we would attack that we are contained by
                foreach (var mineField in Game.MineFields.Where(mineField =>
                    fleet.WillAttack(mineField.Player) &&
                    IsPointInCircle(fleet.Position, mineField.Position, mineField.Radius)))
                {
                    // only sweep one fleet per fleet
                    Sweep(fleet, mineField);
                    break;
                }
            }

            // starbases also sweep
            foreach (var planet in Game.Planets.Where(planet => planet.HasStarbase && planet.Starbase.Aggregate.MineSweep > 0))
            {
                // sweep mines from any minefields we would attack that we are contained by
                foreach (var mineField in Game.MineFields.Where(mineField =>
                    planet.Starbase.WillAttack(mineField.Player) &&
                    IsPointInCircle(planet.Starbase.Position, mineField.Position, mineField.Radius)))
                {
                    // only sweep one fleet per fleet
                    Sweep(planet.Starbase, mineField);
                    break;
                }
            }
        }

        /// <summary>
        /// sweep mines at this minefield
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="mineField"></param>
        internal void Sweep(Fleet fleet, MineField mineField)
        {
            long old = mineField.NumMines;
            mineField.NumMines -= (long)(fleet.Aggregate.MineSweep * Game.Rules.MineFieldStatsByType[mineField.Type].SweepFactor);
            mineField.NumMines = Math.Max(mineField.NumMines, 0);

            long numSwept = old - mineField.NumMines;
            Message.MineFieldSwept(fleet.Player, fleet, mineField, numSwept);
            Message.MineFieldSwept(mineField.Player, fleet, mineField, numSwept);

            if (mineField.NumMines <= 10)
            {
                EventManager.PublishMapObjectDeletedEvent(mineField);
            }
        }
    }
}