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
    public class NetworkServer : Server, IServer
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
        SceneTree sceneTree;

        public override void _Ready()
        {
            base._Ready();

            sceneTree = GetTree();

            sceneTree.Connect("network_peer_connected", this, nameof(OnPlayerConnected));
            sceneTree.Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            rpc = RPC.Instance(sceneTree);
            rpc.PlayerMessageEvent += OnPlayerMessage;
            rpc.RaceUpdatedEvent += OnRaceUpdated;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                if (sceneTree.NativeInstance != IntPtr.Zero)
                {
                    sceneTree.Disconnect("network_peer_connected", this, nameof(OnPlayerConnected));
                    sceneTree.Disconnect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));
                }
                rpc.PlayerMessageEvent -= OnPlayerMessage;
                rpc.RaceUpdatedEvent -= OnRaceUpdated;
            }
        }

        void OnRaceUpdated(Player player, Race race)
        {
            var serverPlayer = players.Find(p => p.Num == player.Num);
            serverPlayer.Race = race;
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
            rpc.SendClientPlayersList(players.Cast<PublicPlayerInfo>().ToList(), networkId);

            // tell this player about themselves
            rpc.SendClientPlayerData(newPlayer);

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

        protected override void PublishGameStartedEvent()
        {
            // tell everyone to start the game
            rpc.SendPostStartGame(Game);
        }

        /// <summary>
        /// After a player has submitted a turn, notify all clients
        /// </summary>
        /// <param name="player"></param>
        protected override void PublishTurnSubmittedEvent(PublicPlayerInfo player)
        {
            rpc.SendTurnSubmitted(player);
        }

        /// <summary>
        /// If a player unsubmitted a turn, notify all clients
        /// </summary>
        /// <param name="player"></param>
        protected override void PublishTurnUnsubmittedEvent(PublicPlayerInfo player)
        {
            rpc.SendTurnUnsubmitted(player);
        }

        protected override void PublishTurnGeneratingEvent()
        {
            rpc.SendTurnGenerating(Game.GameInfo);

        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {
            rpc.SendTurnGeneratorAdvanced(Game.GameInfo, state);
        }

        protected override void PublishTurnPassedEvent()
        {
            // tell everyone we have a new turn and send along their player data
            rpc.SendTurnPassed(Game);
        }

        protected override void PublishGameStartingEvent(PublicGameInfo gameInfo)
        {
            rpc.SendGameStarting(gameInfo);
        }

        #endregion
    }
}