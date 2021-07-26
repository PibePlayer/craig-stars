using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Threading.Tasks;

namespace CraigStars.Server
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

            GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            rpc = RPC.Instance(GetTree());
            rpc.PlayerMessageEvent += OnPlayerMessage;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            GetTree().Disconnect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Disconnect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            rpc.PlayerMessageEvent -= OnPlayerMessage;
        }

        protected override IClientEventPublisher CreateClientEventPublisher()
        {
            // network servers receive client events over RPC
            return RPC.Instance(GetTree());
        }


        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
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

        #region Publishers
        protected override void PublishPlayerUpdatedEvent(PublicPlayerInfo player)
        {
            rpc.SendPlayerUpdated(player);
        }

        protected override void PublishGameStartedEvent()
        {
            // tell everyone to start the game
            rpc.SendPostStartGame(Game);
        }

        protected override void PublishTurnSubmittedEvent(PublicPlayerInfo player)
        {
            rpc.SendPlayerUpdated(player);
        }

        protected override void PublishTurnUnsubmittedEvent(PublicPlayerInfo player)
        {

        }

        protected override void PublishTurnGeneratingEvent()
        {

        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {

        }

        protected override void PublishTurnPassedEvent()
        {
            // tell everyone we have a new turn and send along their player data
            rpc.SendTurnPassed(Game);
        }

        #endregion
    }
}