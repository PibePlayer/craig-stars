
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
        public int MaxSpeed { get; set; }
        public int ChanceOfHit { get; set; }
        public int MinDamagePerFleet { get; set; }
        public int DamagePerEngine { get; set; }
    }
}
