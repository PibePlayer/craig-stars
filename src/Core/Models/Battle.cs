using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The state of a battle as it progresses
    /// </summary>
    public class Battle
    {
        /// <summary>
        /// The tokens for this battle
        /// </summary>
        public List<BattleToken> Tokens { get; set; } = new List<BattleToken>();

        /// <summary>
        /// The rounds of the battle
        /// </summary>
        public List<BattleRecordRound> Rounds { get; set; } = new List<BattleRecordRound>();

        /// <summary>
        /// True if this battle has any tokens that target each other
        /// </summary>
        public bool HasTargets { get; set; }

        /// <summary>
        /// A list of all weapon slots in the battle, sorted by initiative
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BattleWeaponSlot> SortedWeaponSlots
        {
            get => Tokens.SelectMany(
                    token => token.Token.Design.Aggregate.WeaponSlots.Select(
                        slot => new BattleWeaponSlot(token, slot)))
                    .OrderBy(bws => bws.Slot.HullComponent.Initiative);
        }


    }
}