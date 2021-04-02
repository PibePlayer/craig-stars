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
    public class Battle
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Battle));

        public Guid Guid { get; set; } = Guid.NewGuid();

        public Planet Planet { get; set; }
        public Vector2 Position { get; set; }

        /// <summary>
        /// The tokens for this battle
        /// </summary>
        public List<BattleToken> Tokens { get; set; } = new List<BattleToken>();

        /// <summary>
        /// Get an enumerable of remaining tokens
        /// </summary>
        public IEnumerable<BattleToken> RemainingTokens { get => Tokens.Where(token => !(token.Destroyed || token.RanAway)); }

        /// <summary>
        /// Record for players
        /// </summary>
        /// <returns></returns>
        public Dictionary<Player, BattleRecord> PlayerRecords { get; set; } = new Dictionary<Player, BattleRecord>();

        /// <summary>
        /// The current round in battle
        /// </summary>
        /// <value></value>
        public int Round { get; set; }

        /// <summary>
        /// True if this battle has any tokens that target each other
        /// </summary>
        public bool HasTargets { get; set; }

        /// <summary>
        /// A list of weapon slots in the battle, sorted by initiative
        /// </summary>
        /// <typeparam name="BattleWeaponSlot"></typeparam>
        /// <returns></returns>
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
        /// Let the 
        /// </summary>
        internal void RecordNewRound()
        {
            foreach (var record in PlayerRecords.Values)
            {
                // records start with 1 round, so don't re-add it
                if (record.ActionsPerRound.Count <= Round)
                {
                    record.RecordNewRound();
                }
            }
        }

        /// <summary>
        /// Record a move
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordMove(BattleRecordToken token, Vector2 from, Vector2 to)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.RecordMove(Round, token, from, to);
            }
        }

        /// <summary>
        /// Record a token running away
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordRunAway(BattleRecordToken token)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.RecordRunAway(Round, token);
            }
        }

        /// <summary>
        /// Record a token being entired destroyed
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordBeamFire(BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.RecordBeamFire(Round, token, from, to, slot, target, damageDoneShields, damageDoneArmor, tokensDestroyed);
            }

        }

        /// <summary>
        /// Record a token being entired destroyed
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordTorpedoFire(BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed, int hits, int misses)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.RecordTorpedoFire(Round, token, from, to, slot, target, damageDoneShields, damageDoneArmor, tokensDestroyed, hits, misses);
            }

        }
        /// <summary>
        /// Add a token to the battle
        /// </summary>
        /// <param name="token"></param>
        internal void AddToken(BattleToken token)
        {
            Tokens.Add(token);

            var shipToken = token.Token;
            var design = shipToken.Design;

            // also add this token to each player's battle record
            foreach (var reportEntry in PlayerRecords)
            {
                // add a player specific version of this token
                if (reportEntry.Key.DesignsByGuid.TryGetValue(design.Guid, out var playerDesign))
                {
                    reportEntry.Value.Tokens.Add(new BattleRecordToken()
                    {
                        Guid = token.Guid,
                        Owner = token.Owner,
                        Token = new ShipToken(playerDesign, shipToken.Quantity, shipToken.Damage, shipToken.QuantityDamaged)
                    });
                }
                else
                {
                    log.Error($"Could not find Player Design for Battle: {Guid}, Player: {token.Player}, Design: {design.Name}");
                }
            }
        }

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
        /// Setup the player records by guid
        /// </summary>
        internal void SetupPlayerRecords(int numRounds)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.SetupRecord(numRounds);
            }
        }

        internal void RecordStartingPosition(BattleToken token, Vector2 position)
        {
            foreach (var record in PlayerRecords.Values)
            {
                record.TokensByGuid[token.Guid].StartingPosition = position;
            }
        }
    }
}