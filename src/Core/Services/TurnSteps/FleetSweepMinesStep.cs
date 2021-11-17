using System;
using System.Linq;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class FleetSweepMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));
        private readonly FleetService fleetService;

        public FleetSweepMinesStep(IProvider<Game> gameProvider, FleetService fleetService) : base(gameProvider, TurnGenerationState.FleetSweepMinesStep)
        {
            this.fleetService = fleetService;
        }

        public override void Process()
        {
            // Separate our waypoint tasks into groups
            foreach (var fleet in Game.Fleets.Where(fleet => fleet.Aggregate.MineSweep > 0))
            {
                var fleetPlayer = Game.Players[fleet.PlayerNum];
                // sweep mines from any minefields we would attack that we are contained by
                foreach (var mineField in Game.MineFields.Where(mineField =>
                    fleetService.WillAttack(fleet, fleetPlayer, mineField.PlayerNum) &&
                    IsPointInCircle(fleet.Position, mineField.Position, mineField.Radius)))
                {
                    // only sweep one fleet per fleet
                    Sweep(fleet, fleetPlayer, mineField, Game.Players[mineField.PlayerNum]);
                    break;
                }
            }

            // starbases also sweep
            foreach (var planet in Game.Planets.Where(planet => planet.HasStarbase && planet.Starbase.Aggregate.MineSweep > 0))
            {
                var planetPlayer = Game.Players[planet.PlayerNum];
                // sweep mines from any minefields we would attack that we are contained by
                foreach (var mineField in Game.MineFields.Where(mineField =>
                    fleetService.WillAttack(planet.Starbase, planetPlayer, mineField.PlayerNum) &&
                    IsPointInCircle(planet.Starbase.Position, mineField.Position, mineField.Radius)))
                {
                    // only sweep one fleet per fleet
                    Sweep(planet.Starbase, planetPlayer, mineField, Game.Players[mineField.PlayerNum]);
                    break;
                }
            }
        }

        /// <summary>
        /// sweep mines at this minefield
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="mineField"></param>
        internal void Sweep(Fleet fleet, Player fleetPlayer, MineField mineField, Player mineFieldPlayer)
        {
            long old = mineField.NumMines;
            mineField.NumMines -= (long)(fleet.Aggregate.MineSweep * Game.Rules.MineFieldStatsByType[mineField.Type].SweepFactor);
            mineField.NumMines = Math.Max(mineField.NumMines, 0);

            long numSwept = old - mineField.NumMines;
            Message.MineFieldSwept(fleetPlayer, fleet, mineField, numSwept);
            Message.MineFieldSwept(mineFieldPlayer, fleet, mineField, numSwept);

            if (mineField.NumMines <= 10)
            {
                EventManager.PublishMapObjectDeletedEvent(mineField);
            }
        }
    }
}