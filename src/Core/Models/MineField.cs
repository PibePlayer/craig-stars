
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Utils;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Represents a minefield in the universe
    /// </summary>
    [JsonObject(IsReference = true)]
    public class MineField : MapObject
    {
        public int ReportAge = Unexplored;
        public MineFieldType Type { get; set; } = MineFieldType.Standard;

        public bool Detonate { get; set; }
        public long NumMines
        {
            get => numMines;
            set
            {
                numMines = value;
                radius = (float)Math.Sqrt(numMines);
            }
        }
        long numMines;

        /// <summary>
        /// The radius of this minefield, calculated as the square root of the number of mines
        /// </summary>
        public float Radius { get => radius; }
        float radius;

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
        public long GetDecayRate(IEnumerable<Planet> planets = null, Rules rules = null)
        {
            if (Player == null)
            {
                // we can't determine decay rate for minefields we don't own
                return -1;
            }
            // default to using player intel and rules (for UI)
            rules = rules ?? Player.Rules;
            planets = planets ?? Player.AllPlanets;

            var numPlanets = UniverseUtils.GetPlanetsWithin(planets, Position, Radius).Count();
            var decayRate = rules.MineFieldBaseDecayRate;
            decayRate += rules.MineFieldPlanetDecayRate * numPlanets;
            if (Detonate)
            {
                decayRate += rules.MineFieldDetonateDecayRate;
            }

            // Space Demolition mines decay slower
            var decayFactor = Player.Race.PRT == PRT.SD ? rules.SDMinDecayFactor : 1;
            decayRate *= decayFactor;
            decayRate = Math.Min(decayRate, rules.MineFieldMaxDecayRate);

            // we decay at least 10 mines a year for normal and standar mines
            long decayedMines = Math.Max(rules.MineFieldStatsByType[Type].MinDecay, (long)(NumMines * decayRate + .5));
            return decayedMines;
        }
    }
}
