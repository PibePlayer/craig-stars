using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// This service manages damaging fleets hit/detonated by minefields
    /// </summary>
    public class MineFieldDecayer
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldDecayer));

        private readonly IRulesProvider rulesProvider;

        private Rules Rules => rulesProvider.Rules;

        public MineFieldDecayer(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }

        /// <summary>
        /// * The base rate for minefield decay is 2% per year.
        /// * Minefields will decay an additional 4% per planet that is within the field, or 1% per planet for SD races.
        /// * A detonating SD minefield has an additional 25% decay each year.
        /// * Normal and Heavy Minefields have a minimum total decay rate of 10 mines per year
        /// * Speed Bump Minefields have a minimum total decay rate of 2 mines per year
        /// * There is a maximum total decay rate of 50% per year.
        /// </summary>
        /// <param name="planets">A list of planets in the universe (minefields decay when around planets)</param>
        /// <returns></returns>
        public long GetDecayRate(MineField mineField, Player player, IEnumerable<Planet> planets)
        {
            if (!mineField.Owned)
            {
                // we can't determine decay rate for minefields we don't own
                return -1;
            }

            var numPlanets = UniverseUtils.GetPlanetsWithin(planets, mineField.Position, mineField.Radius).Count();
            var decayRate = player.Race.Spec.MineFieldBaseDecayRate;
            decayRate += player.Race.Spec.MineFieldPlanetDecayRate * numPlanets;
            if (mineField.Detonate)
            {
                decayRate += player.Race.Spec.MineFieldDetonateDecayRate;
            }

            // Space Demolition mines decay slower
            var decayFactor = player.Race.Spec.MineFieldMinDecayFactor;
            decayRate *= decayFactor;
            decayRate = Math.Min(decayRate, player.Race.Spec.MineFieldMaxDecayRate);

            // we decay at least 10 mines a year for normal and standar mines
            long decayedMines = Math.Max(Rules.MineFieldStatsByType[mineField.Type].MinDecay, (long)(mineField.NumMines * decayRate + .5));
            return decayedMines;
        }
    }
}

