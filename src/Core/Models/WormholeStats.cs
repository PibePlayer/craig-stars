
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Each Wormhole type has a probability to shift and a max number
    /// of years before it degrades
    /// </summary>
    public struct WormholeStats
    {
        /// <summary>
        /// The number of years this wormhole type will take to degrade
        /// </summary>
        public readonly int yearsToDegrade;

        /// <summary>
        /// The chance during this stability that a wormhole will jump
        /// </summary>
        public readonly float chanceToJump;

        public readonly int jiggleDistance;

        public WormholeStats(int maxYearsToDegrade, float chanceToDegrade, int jiggleDistance = 10)
        {
            this.yearsToDegrade = maxYearsToDegrade;
            this.chanceToJump = chanceToDegrade;
            this.jiggleDistance = jiggleDistance;
        }
    }
}
