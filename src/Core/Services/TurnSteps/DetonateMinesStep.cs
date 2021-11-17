using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// SD can set their mines to detonate, damaging ships inside the radius
    /// </summary>
    public class DetonateMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));

        private readonly MineFieldDamager mineFieldDamager;

        public DetonateMinesStep(IProvider<Game> gameProvider, MineFieldDamager mineFieldDamager) : base(gameProvider, TurnGenerationState.DetonateMinesStep)
        {
            this.mineFieldDamager = mineFieldDamager;
        }

        public override void Process()
        {

            foreach (var mineField in Game.MineFields.Where(mf =>

                Game.Players[mf.PlayerNum].Race.Spec.CanDetonateMineFields &&
                mf.Detonate &&
                Game.Rules.MineFieldStatsByType[mf.Type].CanDetonate))
            {
                Detonate(mineField);
            }
        }

        /// <summary>
        /// Detonate
        /// Damages fleets in the minefield just like they are damaged when moving
        /// </summary>
        /// <param name="mineField"></param>
        internal void Detonate(MineField mineField)
        {
            List<Fleet> fleetsWithin = GetFleetsWithin(mineField.Position, mineField.Radius);
            var stats = Game.Rules.MineFieldStatsByType[mineField.Type];
            var mineFieldPlayer = Game.Players[mineField.PlayerNum];
            foreach (var fleet in fleetsWithin)
            {
                var fleetPlayer = Game.Players[fleet.PlayerNum];
                mineFieldDamager.TakeMineFieldDamage(fleet, fleetPlayer, mineField, mineFieldPlayer, stats, detonating: true);
            }
        }

        /// <summary>
        /// All fleets in the detonation radius take mine damage
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        internal List<Fleet> GetFleetsWithin(Vector2 position, float radius)
        {
            return Game.Fleets.FindAll(fleet => IsPointInCircle(fleet.Position, position, radius)).ToList();
        }

    }
}