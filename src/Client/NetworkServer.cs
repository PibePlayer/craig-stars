using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Threading.Tasks;

namespace CraigStars
{
    /// <summary>
    /// The NetworkServer node is created and added to a new SceneTree when a new multiplayer game is hosted.
    /// This node handles managing a server and handling player joined/left events.
    /// </summary>
    public class NetworkServer : Server
    {
        static CSLog log = LogProvider.GetLogger(typeof(Server));

        /// <summary>
        /// All messages from players
        /// </summary>
        /// <typeparam name="PlayerMessage"></typeparam>
        /// <returns></returns>
        List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        /// <summary>
        /// The players joining the server
        /// </summary>
        List<Player> players = new List<Player>();

        RPC rpc;

        public override void _Ready()
        {
            base._Ready();
            rpc = RPC.Instance(GetTree());

            GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            rpc.PlayerMessageEvent += OnPlayerMessage;
            rpc.GameStartRequestedEvent += OnGameStartRequested;
            rpc.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            GetTree().Disconnect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Disconnect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            rpc.PlayerMessageEvent -= OnPlayerMessage;
            rpc.GameStartRequestedEvent -= OnGameStartRequested;
            rpc.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
        }


        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        protected void OnGameStartRequested()
        {
            // TODO: make our network server have settings
            GameSettings<Player> settings = new GameSettings<Player>()
            {
                Name = "Network Game",
                Size = Size.Small,
                Density = Density.Normal,
                Players = players,
            };

            CreateNewGame(settings);

            // send players their data
            rpc.SendPlayerDataUpdated(Game);
            // tell everyone to start the game
            rpc.SendPostStartGame(Game.GameInfo);
        }


        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        protected override void OnSubmitTurnRequested(Player player)
        {
            base.OnSubmitTurnRequested(player);

            rpc.SendPlayerUpdated(player);
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        /// <returns></returns>
        protected override async Task GenerateNewTurn()
        {
            await base.GenerateNewTurn();

            // send players their data
            rpc.SendPlayerDataUpdated(Game);
            rpc.SendTurnPassed(Game.GameInfo);
        }

        #region Player Connect/Disconnect Events

        void OnPlayerConnected(int networkId)
        {
            // create a new player
            var newPlayer = PlayersManager.CreateNewPlayer(players.Count);

            // claim this player for the network user
            newPlayer.AIControlled = false;
            newPlayer.Ready = false;
            newPlayer.NetworkId = networkId;

            if (players.Count == 0)
            {
                // first player is the host
                newPlayer.Host = true;
            }

            players.Add(newPlayer);

            // let other scenes know about the new player
            log.Info($"{newPlayer} joined game");

            // tell the new player about our other connected players
            rpc.SetPlayers(players);
            // tell other plauyers about the new player
            rpc.SendPlayerJoined(newPlayer);

            // tell this player about the other players
            rpc.SendPlayersUpdated(players.Cast<PublicPlayerInfo>().ToList(), networkId);

            // tell this player about all the messages
            rpc.SendAllMessages(networkId, Messages);
            rpc.SendMessage($"{newPlayer} has joined the game");
        }

        void OnPlayerDisconnected(int networkId)
        {
            // if we are the server, we know a new player has connected
            log.Info($"Player {networkId} disconnected from server.");

            // remove the player from our list
            var player = players.Find(player => player.NetworkId == networkId);
            players.Remove(player);

            // notify clients of the player leaving
            rpc.SetPlayers(players);
            rpc.SendPlayerLeft(player);
            rpc.SendMessage($"{player} has left the game");
        }

        #endregion
    }
}