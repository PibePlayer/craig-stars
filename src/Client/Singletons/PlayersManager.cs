using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;
using log4net;

namespace CraigStars.Singletons
{
    public class PlayersManager : Node
    {
        /// <summary>
        /// This is just a temporary property for development.
        /// </summary>
        public int NumPlayers { get; set; } = 2;

        /// <summary>
        /// The currently active player
        /// </summary>
        /// <value></value>
        public int ActivePlayer
        {
            get => activePlayer;
            set
            {
                activePlayer = value;
                me = null;
            }
        }
        int activePlayer = 0;

        CSLog log = LogProvider.GetLogger(typeof(PlayersManager));

        public Color[] PlayerColors { get; } = new Color[] {
            new Color("c33232"),
            new Color("1f8ba7"),
            new Color("43a43e"),
            new Color("8d29cb"),
            new Color("b88628")
        };

        string[] playerNames = new string[] {
            "Craig",
            "Ted",
            "Joe",
            "Bob",
            "Lance",
            "Elias",
            "Eva",
            "Maeve",
        };

        string[] raceNames = new string[] {
            "Berserker",
            "Hooveron",
            "American",
            "Ubert",
            "Kurkonian",
            "Mensoid",
            "Ubert",
            "Crusher",
            "House Cat",
            "Bulushi",
            "Ferret",
            "Nee",
            "Golem",
            "Loraxoid",
            "Hicardi",
            "Nairnian",
            "Hawk",
            "Rush'n",
            "Nee",
            "Tritizoid",
        };

        /// <summary>
        /// Data for all the players in the game
        /// </summary>
        /// <typeparam name="Player"></typeparam>
        /// <returns></returns>
        public List<PublicPlayerInfo> Players { get; } = new List<PublicPlayerInfo>();

        /// <summary>
        /// Messages from ourselves and other players
        /// </summary>
        /// <typeparam name="PlayerMessage"></typeparam>
        /// <returns></returns>
        public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        public Dictionary<int, Player> PlayersByNetworkId { get; } = new Dictionary<int, Player>();

        private Player me;
        public static Player Me
        {
            get
            {
                if (Instance.me == null)
                {
                    if (Instance.IsMultiplayer())
                    {
                        Instance.me = Instance.Players.Find(p => p.NetworkId == Instance.GetTree().GetNetworkUniqueId()) as Player;
                    }
                    else
                    {
                        // no network, we are player one
                        if (Instance.ActivePlayer >= 0 && Instance.ActivePlayer < Instance.Players.Count)
                        {
                            Instance.me = Instance.Players[Instance.ActivePlayer] as Player;
                        }
                    }
                }
                return Instance.me;
            }
            set
            {
                Instance.me = value;
                Instance.Players[value.Num] = value;
            }
        }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static PlayersManager instance;
        public static PlayersManager Instance
        {
            get
            {
                return instance;
            }
        }

        PlayersManager()
        {
            instance = this;
        }

        public override void _Ready()
        {
            // Subscribe to some player joined/left events
            Signals.TurnSubmittedEvent += OnTurnSubmitted;
            Signals.PlayerJoinedEvent += OnPlayerJoined;
            Signals.PlayerLeftEvent += OnPlayerLeft;
            Signals.PlayerUpdatedEvent += OnPlayerUpdated;
            Signals.PlayerMessageEvent += OnPlayerMessage;
        }

        void OnTurnSubmitted(PublicPlayerInfo player)
        {
            GetPlayer(player.Num).SubmittedTurn = true;
        }

        public override void _ExitTree()
        {
            Signals.TurnSubmittedEvent += OnTurnSubmitted;
            Signals.PlayerJoinedEvent -= OnPlayerJoined;
            Signals.PlayerLeftEvent -= OnPlayerLeft;
            Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
            Signals.PlayerMessageEvent -= OnPlayerMessage;
        }

        public void Reset()
        {
            me = null;
            Players.Clear();
            PlayersByNetworkId.Clear();
            Messages.Clear();
            RulesManager.Rules.Random.Shuffle(raceNames);
            RulesManager.Rules.Random.Shuffle(playerNames);
        }

        /// <summary>
        /// Setup the initial players list with players based on color
        /// </summary>
        public void SetupPlayers()
        {
            me = null;
            var rules = RulesManager.Rules;
            for (var num = 0; num < NumPlayers; num++)
            {
                var player = new Player
                {
                    Num = num,
                    Name = num < playerNames.Length ? playerNames[num] : $"Player {num + 1}",
                    Color = num < PlayerColors.Length ? PlayerColors[num] : new Color((float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble()),
                    AIControlled = num != 0,
                    Ready = true,
                    TechStore = TechStore.Instance,
                    DefaultHullSet = num % 2
                };

                // TODO: don't enable this by default in regular games
                // player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name));

                if (player.AIControlled)
                {
                    player.Race.Name = num < raceNames.Length ? raceNames[num] : $"Race {num + 1}";
                    player.Race.PluralName = player.Race.Name + "s";
                }

                Players.Add(player);

                Signals.PublishPlayerUpdatedEvent(Players[num]);
            }
        }

        /// <summary>
        /// Add a new player to PlayersManager
        /// </summary>
        /// <returns></returns>
        public Player AddNewPlayer()
        {
            var num = Players.Count;
            var player = new Player
            {
                Num = num,
                Name = num < playerNames.Length ? playerNames[num] : $"Player {num + 1}",
                Color = num < PlayerColors.Length ? PlayerColors[num] : new Color((float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble()),
                AIControlled = num != 0,
                Ready = true,
                TechStore = TechStore.Instance,
                DefaultHullSet = num % 2
            };

            if (player.AIControlled)
            {
                player.Race.Name = num < raceNames.Length ? raceNames[num] : $"Race {num + 1}";
                player.Race.PluralName = player.Race.Name + "s";
            }

            Players.Add(player);
            Signals.PublishPlayerUpdatedEvent(Players[num]);

            return player;
        }

        /// <summary>
        /// After loading a game, setup the PlayersManager with the players from the game
        /// </summary>
        /// <param name="players"></param>
        public void InitPlayersFromGame(List<Player> players)
        {
            me = null;
            Players.Clear();
            PlayersByNetworkId.Clear();
            Messages.Clear();
            Players.AddRange(players);
            NumPlayers = Players.Count;
        }

        #region Event Listeners

        /// <summary>
        /// A player has joined, find them a match in our players list and notify any
        /// listeners that we have a new human player
        /// </summary>
        /// <param name="networkId"></param>
        private void OnPlayerJoined(int networkId)
        {
            // find an AI controlled player
            // unless we are the server, in which case we claim player 1
            var emptyPlayer = networkId == 1 ? Players[0] : Players.Find(p => p.AIControlled);

            if (emptyPlayer != null)
            {
                // claim this player for the network user
                emptyPlayer.AIControlled = false;
                emptyPlayer.Ready = false;
                emptyPlayer.NetworkId = networkId;

                log.Info($"{emptyPlayer} joined and is added to the player registry");
                Signals.PublishPlayerUpdatedEvent(emptyPlayer);
            }
            else
            {
                log.Error($"Player with networkId {networkId} tried to join, but we couldn't find any empty player slots!");
            }
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        public PublicPlayerInfo GetPlayer(int playerNum)
        {
            if (playerNum >= 0 && playerNum < Players.Count)
            {
                return Players[playerNum];
            }
            return null;
        }

        public PublicPlayerInfo GetNetworkPlayer(int networkId)
        {
            return Players.Find(p => p.NetworkId == networkId);
        }

        /// <summary>
        /// If a player leaves, remove them from our registry and notify any listeners that we
        /// have a new AI player
        /// </summary>
        /// <param name="networkId"></param>
        private void OnPlayerLeft(int networkId)
        {
            var player = Players.Find(p => p.NetworkId == networkId);
            if (player != null)
            {
                log.Info($"{player} left and will be removed from the player registry");
                player.AIControlled = true;
                player.NetworkId = 0;
                Signals.PublishPlayerUpdatedEvent(player);
            }
            else
            {
                log.Error($"Player with networkId {networkId} left the game, but they werne't in our player registry!");
            }
        }

        private void OnPlayerUpdated(PublicPlayerInfo player)
        {
            var existingPlayer = GetPlayer(player.Num);
            if (existingPlayer != null)
            {
                existingPlayer.Update(player);
            }
            else
            {
                if (player.NetworkId == GetTree().GetNetworkUniqueId())
                {
                    log.Debug($"The server sent along my Player info. Registering full Player for {player} in PlayersManager.");
                    var fullPlayer = new Player()
                    {
                        TechStore = TechStore.Instance
                    };
                    fullPlayer.Update(player);
                    Players.Add(fullPlayer);
                    me = null;
                }
                else
                {
                    log.Debug($"Registering Player info for {player} in PlayersManager.");
                    Players.Add(player);
                }
            }
        }

        #endregion

    }
}
