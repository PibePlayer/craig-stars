using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A recording of a single battle
    /// </summary>
    public class BattleRecord : Discoverable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleRecord));


        public Guid Guid { get; set; } = Guid.NewGuid();

        public PublicPlayerInfo Owner
        {
            get
            {
                if (Player != null)
                {
                    owner = Player;
                }
                return owner;
            }
            set
            {
                owner = value;
            }
        }
        PublicPlayerInfo owner;

        /// <summary>
        /// For fleets we own, the Player field is populated
        /// Otherwise, the Owner field is populated
        /// </summary>
        [JsonIgnore]
        public Player Player { get; set; }

        /// <summary>
        /// The tokens for this battle
        /// </summary>
        public List<BattleRecordToken> Tokens { get; set; } = new List<BattleRecordToken>();

        /// <summary>
        /// The actions taken per round
        /// </summary>
        public List<List<BattleRecordTokenAction>> ActionsPerRound { get; set; } = new List<List<BattleRecordTokenAction>>();

        // stats
        public int MyShipCount { get; set; }
        public int EnemyShipCount { get; set; }
        public int FriendShipCount { get; set; }
        public int MyShipsDestroyed { get; set; }
        public int FriendShipsDestroyed { get; set; }
        public int EnemyShipsDestroyd { get; set; }

        /// <summary>
        /// Store a list of our tokens by guid
        /// </summary>
        /// <typeparam name="Guid"></typeparam>
        /// <typeparam name="BattleRecordToken"></typeparam>
        /// <returns></returns>
        [JsonIgnore]
        public Dictionary<Guid, BattleRecordToken> TokensByGuid { get; set; } = new Dictionary<Guid, BattleRecordToken>();

        /// <summary>
        /// Populate a lookup table of items by guid
        /// </summary>
        public void SetupRecord(int numRounds)
        {
            TokensByGuid = Tokens.ToLookup(token => token.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            foreach (var token in Tokens)
            {
                if (token.Owner.Num == Owner.Num)
                {
                    MyShipCount++;
                }
                else if (Player.IsFriend(token.Owner))
                {
                    FriendShipCount++;
                }
                else
                {
                    EnemyShipCount++;
                }
            }
            // always start with one round
            ActionsPerRound.Clear();
            ActionsPerRound.Add(new List<BattleRecordTokenAction>());
        }

        /// <summary>
        /// Add a new round to the record
        /// </summary>
        internal void RecordNewRound()
        {
            ActionsPerRound.Add(new List<BattleRecordTokenAction>());
        }

        /// <summary>
        /// Record a move
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordMove(int round, BattleRecordToken token, Vector2 from, Vector2 to)
        {
            ActionsPerRound[ActionsPerRound.Count - 1].Add(new BattleRecordTokenMove(TokensByGuid[token.Guid], from, to));
            if (Owner.Num == 0)
            {
                log.Debug($"Round: {round} {ActionsPerRound[round][ActionsPerRound[round].Count - 1]}");
            }
        }

        /// <summary>
        /// Record a token running away
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordRunAway(int round, BattleRecordToken token)
        {
            ActionsPerRound[ActionsPerRound.Count - 1].Add(new BattleRecordTokenRanAway(TokensByGuid[token.Guid]));
            if (Owner.Num == 0)
            {
                log.Debug($"Round: {round} {ActionsPerRound[round][ActionsPerRound[round].Count - 1]}");
            }
        }

        /// <summary>
        /// Record a token firing a beam weapon
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordBeamFire(int round, BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed)
        {
            if (target.Owner.Num == Owner.Num)
            {
                MyShipCount++;
            }
            else if (Player.IsFriend(target.Owner))
            {
                FriendShipCount++;
            }
            else
            {
                EnemyShipCount++;
            }
            ActionsPerRound[ActionsPerRound.Count - 1].Add(new BattleRecordTokenBeamFire(TokensByGuid[token.Guid], from, to, slot, TokensByGuid[target.Guid], damageDoneShields, damageDoneArmor, tokensDestroyed));
            if (Owner.Num == 0)
            {
                log.Debug($"Round: {round} {ActionsPerRound[round][ActionsPerRound[round].Count - 1]}");
            }
        }

        /// <summary>
        /// Record a token firing a salvo of torpedos
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void RecordTorpedoFire(int round, BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed, int hits, int misses)
        {
            if (target.Owner.Num == Owner.Num)
            {
                MyShipCount++;
            }
            else if (Player.IsFriend(target.Owner))
            {
                FriendShipCount++;
            }
            else
            {
                EnemyShipCount++;
            }
            ActionsPerRound[ActionsPerRound.Count - 1].Add(new BattleRecordTokenTorpedoFire(TokensByGuid[token.Guid], from, to, slot, TokensByGuid[target.Guid], damageDoneShields, damageDoneArmor, tokensDestroyed, hits, misses));
            if (Owner.Num == 0)
            {
                log.Debug($"Round: {round} {ActionsPerRound[round][ActionsPerRound[round].Count - 1]}");
            }
        }
    }
}