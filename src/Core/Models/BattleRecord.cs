using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A recording of a single battle
    /// </summary>
    public class BattleRecord<T> where T : BattleRecordToken
    {
        /// <summary>
        /// The tokens for this battle
        /// </summary>
        public List<T> Tokens { get; set; } = new List<T>();

        /// <summary>
        /// The rounds of the battle
        /// </summary>
        public List<BattleRecordTokenAction> Actions { get; set; } = new List<BattleRecordTokenAction>();

    }
}