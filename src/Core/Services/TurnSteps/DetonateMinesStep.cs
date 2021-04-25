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
    /// SD can set their mines to detonate, damaging ships inside the radius
    /// </summary>
    public class DetonateMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));

        MineFieldDamager mineFieldDamager = new MineFieldDamager();

        public DetonateMinesStep(Game game) : base(game, TurnGenerationState.DetonateMines) { }

        public override void Process()
        {

            foreach (var mineField in Game.MineFields.Where(mf =>
                mf.Player.Race.PRT == PRT.SD &&
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
            foreach (var fleet in fleetsWithin)
            {
                mineFieldDamager.TakeMineFieldDamage(fleet, mineField, stats, detonating: true);
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