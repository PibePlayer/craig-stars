using System.Collections.Generic;
using Godot;

namespace CraigStars.Singletons
{
    public class PlayersManager : Node
    {
        /// <summary>
        /// Data for all the players in the game
        /// </summary>
        /// <typeparam name="Player"></typeparam>
        /// <returns></returns>
        public List<Player> Players { get; } = new List<Player>();

        /// <summary>
        /// Messages from ourselves and other players
        /// </summary>
        /// <typeparam name="PlayerMessage"></typeparam>
        /// <returns></returns>
        public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        public Dictionary<int, Player> PlayersByNetworkId { get; } = new Dictionary<int, Player>();

        private Player me;
        public Player Me
        {
            get
            {
                if (me == null)
                {
                    if (GetTree().HasNetworkPeer())
                    {
                        me = Players.Find(p => p.NetworkId == GetTree().GetNetworkUniqueId());
                    }
                    else
                    {
                        // no network, we are player one
                        me = Players[0];
                    }
                }
                return me;
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
            SetupPlayers(new UniverseSettings());
            // Subscribe to some player joined/left events
            Signals.PlayerJoinedEvent += OnPlayerJoined;
            Signals.PlayerLeftEvent += OnPlayerLeft;
            Signals.PlayerUpdatedEvent += OnPlayerUpdated;
            Signals.PlayerMessageEvent += OnPlayerMessage;
        }

        public override void _ExitTree()
        {
            Signals.PlayerJoinedEvent -= OnPlayerJoined;
            Signals.PlayerLeftEvent -= OnPlayerLeft;
            Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
            Signals.PlayerMessageEvent -= OnPlayerMessage;
        }

        public void Reset(UniverseSettings settings)
        {
            Players.ForEach(p => p.QueueFree());
            Players.Clear();
            PlayersByNetworkId.Clear();
            Messages.Clear();
            SetupPlayers(settings);
        }

        /// <summary>
        /// Setup the initial players list with players based on color
        /// </summary>
        public void SetupPlayers(UniverseSettings settings)
        {
            for (var num = 0; num < settings.NumPlayers; num++)
            {
                var player = new Player
                {
                    Num = num,
                    PlayerName = $"Player {num + 1}",
                    Color = settings.PlayerColors[num],
                    AIControlled = true,
                    Ready = true,
                };

                Players.Add(player);
                AddChild(player);

                Signals.PublishPlayerUpdatedEvent(Players[num]);
            }
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
            var emptyPlayer = Players.Find(p => p.AIControlled);
            if (emptyPlayer != null)
            {
                // claim this player for the network user
                emptyPlayer.AIControlled = false;
                emptyPlayer.Ready = false;
                emptyPlayer.NetworkId = networkId;

                GD.Print($"{emptyPlayer} joined and is added to the player registry");
                Signals.PublishPlayerUpdatedEvent(emptyPlayer);
            }
            else
            {
                GD.PrintErr($"Player with networkId {networkId} tried to join, but we couldn't find any empty player slots!");
            }
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        public Player GetPlayer(int playerNum)
        {
            if (playerNum > 0 && playerNum <= Players.Count)
            {
                return Players[playerNum - 1];
            }
            return null;
        }

        public Player GetNetworkPlayer(int networkId)
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
                GD.Print($"{player} left and will be removed from the player registry");
                player.AIControlled = true;
                player.NetworkId = 0;
                Signals.PublishPlayerUpdatedEvent(player);
            }
            else
            {
                GD.PrintErr($"Player with networkId {networkId} left the game, but they werne't in our player registry!");
            }
        }

        private void OnPlayerUpdated(Player player)
        {
            var existingPlayer = GetPlayer(player.Num);
            if (existingPlayer != null)
            {
                // existingPlayer.From(player);
            }
        }

        #endregion

    }
}
