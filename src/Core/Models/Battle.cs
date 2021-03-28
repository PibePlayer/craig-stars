using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The state of a battle as it progresses
    /// </summary>
    public class Battle : BattleRecord<BattleToken>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Battle));

        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// True if this battle has any tokens that target each other
        /// </summary>
        public bool HasTargets { get; set; }

        public List<BattleWeaponSlot> SortedWeaponSlots { get; private set; } = new List<BattleWeaponSlot>();

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
        /// Build the SortedWeaponSlots property
        /// </summary>
        public void BuildSortedWeaponSlots()
        {
            SortedWeaponSlots = Tokens.SelectMany(
                    token => token.Token.Design.Aggregate.WeaponSlots.Select(
                        slot => new BattleWeaponSlot(token, slot)))
                    .OrderBy(bws => bws.Slot.HullComponent.Initiative).ToList();
        }

        /// <summary>
        /// Record a move
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordMove(BattleRecordToken token, Vector2 from, Vector2 to)
        {
            Actions.Add(new BattleRecordTokenMove(token, from, to));
            log.Debug(Actions[Actions.Count - 1]);
        }

        /// <summary>
        /// Record a token running away
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordRunAway(BattleRecordToken token)
        {
            Actions.Add(new BattleRecordTokenRanAway(token));
            log.Debug(Actions[Actions.Count - 1]);
        }

        /// <summary>
        /// Record a token being entired destroyed
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordFire(BattleRecordToken token, Vector2 from, int slot, BattleRecordToken target, int damage, int tokensDestroyed)
        {
            Actions.Add(new BattleRecordTokenFire(token, from, slot, target, damage, tokensDestroyed));
            log.Debug(Actions[Actions.Count - 1]);
        }

        /// <summary>
        /// Record a move
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordDestroyed(BattleRecordToken token)
        {
            Actions.Add(new BattleRecordTokenDestroyed(token));
        }

    }
}