
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Each MineField type has certain damage/chance of hit stats
    /// MineField layers lay similar mine field types at different rates
    /// </summary>
    public class MineFieldStats
    {
        public int MinDamagePerFleetRS { get; set; }
        public int DamagePerEngineRS { get; set; }

        /// <summary>
        /// The max safe speed to travel through a minefield
        /// </summary>
        public int MaxSpeed { get; set; }

        /// <summary>
        /// The chance to hit a mine in this minefield, per warp speed above safe, i.e.
        /// for a fleet travelling warp 9 through a minefield with a MaxSpeed of 4, chance to hit of .003
        /// a chance of .003 * 5 warp = 1.5% chance to hit per light year travelled
        /// </summary>
        public float ChanceOfHit { get; set; }
        public int MinDamagePerFleet { get; set; }
        public int DamagePerEngine { get; set; }

        /// <summary>
        /// Speed Hump mines are harder to sweep
        /// </summary>
        public float SweepFactor { get; set; } = 1f;

        /// <summary>
        /// The minimum number of mines that decay each year
        /// </summary>
        public int MinDecay { get; set; } = 0;

        public bool CanDetonate { get; set; } = false;

    }
}
