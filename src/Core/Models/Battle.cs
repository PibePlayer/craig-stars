using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The state of a battle as it progresses
    /// </summary>
    public class Battle : BattleRecord<BattleToken>
    {
        public Guid Guid { get; set; } = new Guid();

        /// <summary>
        /// True if this battle has any tokens that target each other
        /// </summary>
        public bool HasTargets { get; set; }

        /// <summary>
        /// Movement is split into 4 round blocks. Each block contains a list of tokens
        /// that are moved during that round. This list repeats for each 4 rounds of battle
        /// i.e. it repeats 4 times for 16 rounds
        /// </summary>
        public List<BattleToken>[] MoveOrder = new List<BattleToken>[4] {
            new List<BattleToken>(),
            new List<BattleToken>(),
            new List<BattleToken>(),
            new List<BattleToken>(),
        };

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