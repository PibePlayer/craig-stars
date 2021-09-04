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

        RPC rpc;
        SceneTree sceneTree;

        public override void _Ready()
        {
            base._Ready();

            sceneTree = GetTree();

            rpc = RPC.Instance(sceneTree);
            rpc.PlayerMessageEvent += OnPlayerMessage;
            rpc.RaceUpdatedEvent += OnRaceUpdated;
            rpc.AddAIPlayerEvent += OnAddAIPlayer;
            rpc.KickPlayerEvent += OnKickPlayer;

            sceneTree.Connect("network_peer_connected", this, nameof(OnPlayerConnected));
            sceneTree.Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

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
                rpc.AddAIPlayerEvent -= OnAddAIPlayer;
                rpc.KickPlayerEvent -= OnKickPlayer;
            }
        }

        protected override IClientEventPublisher CreateClientEventPublisher()
        {
            // network servers receive client events over RPC
            return RPC.Instance(GetTree());
        }


        public override Game LoadGame(string gameName, int year, bool multithreaded, bool saveToDisk)
        {
            var game = base.LoadGame(gameName, year, multithreaded, saveToDisk);

            // clear out network ids for loaded players
            // when we load the game, the player needs to join on again
            game.Players.ForEach(p => p.NetworkId = 0);
            game.GameInfo.Players.ForEach(p => p.NetworkId = 0);

            return game;
        }

        #region Game Setup Events

        void OnAddAIPlayer()
        {
            // create a new player
            var newPlayer = PlayersManager.CreateNewPlayer(rpc.Players.Count);

            // claim this player for the network user
            newPlayer.AIControlled = true;
            newPlayer.Ready = true;

            rpc.Players.Add(newPlayer);

            // let other scenes know about the new player
            log.Info($"AI: {newPlayer} joined game");

            // tell other plauyers about the new player
            rpc.ServerSendClientPlayerJoinedNewGame(newPlayer);

            // tell this player about themselves
            rpc.ServerSendClientPlayerData(null, newPlayer);

            // tell this player about all the messages
            rpc.SendMessage($"{newPlayer} (AI) has joined the game");
        }

        void OnRaceUpdated(Player player, Race race)
        {
            var serverPlayer = rpc.Players.Find(p => p.Num == player.Num);
            serverPlayer.Race = race;
        }

        #endregion


        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        #region Player Connect/Disconnect Events

        void OnPlayerConnected(int networkId)
        {
            // if we have a game in progress, send the player the game info and let them
            // choose which player to play
            if (Settings != null && Settings.ContinueGame)
            {
                if (Game != null)
                {
                    log.Info("Player joined existing game, sending PlayerJoinedExistingGameEvent");
                    rpc.ServerSendClientPlayerJoinedExistingGame(networkId, Game.GameInfo);
                }
                else
                {
                    log.Info("Player joined game as it was being loaded, added networkId to RPC.NetworkIds");
                    RPC.Instance(GetTree()).NetworkIds.Add(networkId);
                }
            }
            else
            {
                // create a new player
                var newPlayer = PlayersManager.CreateNewPlayer(rpc.Players.Count);

                // claim this player for the network user
                newPlayer.AIControlled = false;
                newPlayer.Ready = false;
                newPlayer.NetworkId = networkId;

                if (rpc.Players.Count == 0)
                {
                    // first player is the host
                    newPlayer.Host = true;
                }

                rpc.Players.Add(newPlayer);

                // let other scenes know about the new player
                log.Info($"{newPlayer} joined game");

                // tell other plauyers about the new player
                rpc.ServerSendClientPlayerJoinedNewGame(newPlayer);

                // tell this player about the other players
                rpc.SendClientPlayersList(rpc.Players.Cast<PublicPlayerInfo>().ToList(), networkId);

                // tell this player about themselves
                rpc.ServerSendClientPlayerData(null, newPlayer);

                // tell this player about all the messages
                rpc.ServerSendClientAllMessages(networkId, Messages);
                rpc.SendMessage($"{newPlayer} has joined the game");
            }

        }

        void OnPlayerDisconnected(int networkId)
        {
            // if we are the server, we know a new player has connected
            log.Info($"Player {networkId} disconnected from server.");

            // remove the player from our list
            var player = rpc.Players.Find(player => player.NetworkId == networkId);
            rpc.Players.Remove(player);
            rpc.Players.Each((player, index) => player.Num = index);

            // notify clients of the player leaving
            rpc.ServerSendClientPlayerLeft(player, rpc.Players.Cast<PublicPlayerInfo>().ToList());
            rpc.SendMessage($"{player} has left the game");
        }

        void OnKickPlayer(int playerNum)
        {
            // if we are the server, we know a new player has connected
            log.Info($"Player {playerNum} has been kicked from the server.");

            // remove the player from our list
            var player = rpc.Players.Find(player => player.Num == playerNum);
            rpc.Players.Remove(player);
            rpc.Players.Each((player, index) => player.Num = index);

            if (player.NetworkId != 0)
            {
                var peer = GetTree().NetworkPeer as NetworkedMultiplayerENet;
                peer.DisconnectPeer(player.NetworkId);
            }

            // notify clients of the player leaving
            rpc.ServerSendClientPlayerLeft(player, rpc.Players.Cast<PublicPlayerInfo>().ToList());
            rpc.SendMessage($"{player} has left the game");
        }
        #endregion

        #region Publishers

        protected override void PublishPlayerDataEvent(Player player)
        {
            // tell everyone to start the game
            rpc.ServerSendClientPlayerData(Game.GameInfo, player);
        }


        protected override void PublishGameStartedEvent()
        {
            List<int> networkIds = new(rpc.NetworkIds);
            foreach (int networkId in networkIds)
            {
                rpc.ServerSendClientPlayerJoinedExistingGame(networkId, Game.GameInfo);
            }
            networkIds.ForEach(networkId => rpc.NetworkIds.Remove(networkId));

            // update the rpc's list of players  with players from this game, in case
            // we just loaded it from disk
            rpc.Players = new(Game.Players);

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