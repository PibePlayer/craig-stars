using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A recording of a single battle
    /// </summary>
    public class BattleRecord
    {
        /// <summary>
        /// The tokens for this battle
        /// </summary>
        public List<BattleToken> Tokens { get; set; } = new List<BattleToken>();

        /// <summary>
        /// The rounds of the battle
        /// </summary>
        public List<BattleRecordRound> Rounds { get; set; } = new List<BattleRecordRound>();

    }
}