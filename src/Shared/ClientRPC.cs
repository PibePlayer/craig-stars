using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Client;
using CraigStars.Server;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Both servers and clients have a ClientRPC and ServerRPC node. 
    /// 
    /// ClientRPC calls are called by the server, executed on the client
    /// ServerRPC calls are called by the client, executed on the server.
    /// The ClientRPC also publishes events to the clients so the client can respond to RPC calls from the server
    public class ClientRPC : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(ClientRPC));

        #region RPC events

        public event Action<PlayerMessage> PlayerMessageEvent;
        public event Action<PublicGameInfo, Player> ServerPlayerDataEvent;
        public event Action<List<PublicPlayerInfo>> ServerPlayersListEvent;
        public event Action<PublicPlayerInfo> PlayerJoinedNewGameEvent;
        public event Action<PublicGameInfo> PlayerJoinedExistingGameEvent;
        public event Action<PublicPlayerInfo, List<PublicPlayerInfo>> PlayerLeftEvent;

        #endregion

        /// <summary>
        /// We have a uniqe RPC instance per scene tree
        /// </summary>
        public static ClientRPC Instance(SceneTree sceneTree)
        {
            return sceneTree.Root.GetNode<ClientRPC>("/root/ClientRPC");
        }

        #region Broadcasts to All Network Peers

        public void SendMessage(string message, int playerNum = PlayerMessage.ServerPlayerNum)
        {
            var playerMessage = new PlayerMessage(playerNum, message);
            log.Debug($"{(this.IsServer() ? "Server:" : "Client:")} Sending Message {playerMessage}");
            Rpc(nameof(Message), Serializers.Serialize(playerMessage));
        }

        /// <summary>
        /// Send a client 
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="messages"></param>
        public void SendAllMessages(int networkId, List<PlayerMessage> messages)
        {
            log.Debug($"{(this.IsServer() ? "Server:" : "Client:")} Sending All Messages to {networkId}");
            messages.ForEach(m => RpcId(networkId, nameof(Message), Serializers.Serialize(m)));
        }

        [RemoteSync]
        public void Message(string json)
        {
            var logPrefix = this.IsServer() ? "Server:" : "Client:";
            var message = Serializers.Deserialize<PlayerMessage>(json);
            if (message.HasValue)
            {
                log.Debug($"{logPrefix} Received PlayerMessage {message} from {GetTree().GetRpcSenderId()}");

                // we received a new message, notify listeners
                // Hosts actually receive this message twice, once as a client and once as a server
                // TODO: We probably need some way to tell if we are a headless server vs an host server. If this is a headless server we want to publish this message
                if (!this.IsServer())
                {
                    PlayerMessageEvent?.Invoke(message.Value);
                }
            }
            else
            {
                log.Error($"{logPrefix} Failed to parse json: {json}");
            }
        }

        #endregion

        #region Game Setup

        /// <summary>
        /// Send a message to all clients that a new player joined
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerJoinedNewGame(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"Notifying clients about player join: {player}");
            var playerJson = Serializers.Serialize(player);
            Rpc(nameof(PlayerJoinedNewGame), playerJson);
        }

        [Remote]
        public void PlayerJoinedNewGame(string playerJson)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"Received PlayerJoined event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerJoinedNewGameEvent?.Invoke(player);
            }
            else
            {
                log.Error($"Failed to parse json: {playerJson}");
            }
        }

        /// <summary>
        /// If a network game is in progress, send the newly joined player the gameInfo so they
        /// can resume playing a player
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerJoinedExistingGame(int networkId, PublicGameInfo gameInfo)
        {
            // send our peers an update of a player
            log.Debug($"Notifying new player about game: {gameInfo}");
            var gameInfoJson = Serializers.Serialize(gameInfo);
            RpcId(networkId, nameof(PlayerJoinedExistingGame), gameInfoJson);
        }

        [Remote]
        public void PlayerJoinedExistingGame(string gameInfoJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            if (gameInfo != null)
            {
                log.Debug($"Received PlayerJoinedExistingGame event from server.");

                // notify listeners that we have updated Player
                PlayerJoinedExistingGameEvent?.Invoke(gameInfo);
            }
            else
            {
                log.Error($"Failed to parse json: {gameInfoJson}");
            }
        }

        /// <summary>
        /// Send a message to all clients that a player left
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerLeft(PublicPlayerInfo player, List<PublicPlayerInfo> remainingPlayers)
        {
            // send our peers an update of a player
            log.Debug($"Notifying clients about player left: {player}");
            Rpc(nameof(PlayerLeft), Serializers.Serialize(player), Serializers.Serialize(remainingPlayers));
        }

        [Remote]
        public void PlayerLeft(string playerJson, string remainingPlayersJson)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            var remainingPlayers = Serializers.DeserializeObject<List<PublicPlayerInfo>>(remainingPlayersJson);
            if (player != null)
            {
                log.Debug($"Received PlayerLeft event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerLeftEvent?.Invoke(player, remainingPlayers);
            }
            else
            {
                log.Error($"Failed to parse json: {playerJson}");
            }
        }

        /// <summary>
        /// Called by the server to notify all clients about new player data
        /// </summary>
        /// <param name="players"></param>
        /// <param name="networkId"></param>
        public void SendPlayersListUpdate(List<PublicPlayerInfo> players, int networkId)
        {
            // servers listen for signals and notify clients
            if (this.IsServer())
            {
                var playersArray = new Godot.Collections.Array(players.Select(p => Serializers.Serialize(p)).ToArray());
                log.Debug($"Sending player infos to network player: {networkId}");
                // we are a server, tell the clients we have a player update
                RpcId(networkId, nameof(PlayersListUpdated), playersArray);
            }
            else
            {
                log.Error("A client tried to send a list of all players over Rpc");
            }
        }

        /// <summary>
        /// Method called by the server whenever a client needs to know about player updates
        /// </summary>
        /// <param name="data"></param>
        [Remote]
        public void PlayersListUpdated(Godot.Collections.Array jsons)
        {
            var players = new List<PublicPlayerInfo>(jsons.Count);
            foreach (string json in jsons)
            {
                var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
                if (player != null)
                {
                    log.Debug($"Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                    players.Add(player);
                }
                else
                {
                    log.Error($"Failed to parse json: {json}");
                }
            }

            // notify listeners that we have updated Player
            ServerPlayersListEvent?.Invoke(players);
        }

        #endregion

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerData(PublicGameInfo gameInfo, Player player, int networkId)
        {
            var players = gameInfo != null ? gameInfo.Players : new List<PublicPlayerInfo>() { player };
            var settings = Serializers.CreatePlayerSettings(TechStore.Instance);

            log.Info($"Server: Sending {player} updated player data");
            string json = Serializers.Serialize(player, settings);
            string gameInfoJson = gameInfo != null ? Serializers.Serialize(gameInfo) : null;

            RpcId(networkId, nameof(PlayerData), gameInfoJson, player.Num, json);
        }

        [Remote]
        public void PlayerData(string gameInfoJson, int playerNum, string playerJson)
        {
            log.Info("Client: Server sent player data");
            PublicGameInfo gameInfo = gameInfoJson != null ? Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson) : null;

            var player = Serializers.DeserializeObject<Player>(playerJson, Serializers.CreatePlayerSettings(TechStore.Instance));
            log.Info("Client: Done recieving updated player data");

            // notify any listeners that we have new data for this player
            ServerPlayerDataEvent?.Invoke(gameInfo, player);
            Client.EventManager.PublishPlayerDataEvent(gameInfo, player);
        }

        public void SendGameStarting(PublicGameInfo gameInfo)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"Sending GameStarting to players");
            Rpc(nameof(GameStarting), gameInfoJson);
        }

        [Remote]
        public void GameStarting(string gameInfoJson)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishGameStartingEvent(gameInfo);
            log.Info($"Received GameStarting");
        }

        /// <summary>
        /// Sent by the server to notify players the game is started
        /// </summary>
        /// <param name="year"></param>
        /// <param name="networkId"></param>
        public void SendGameStarted(Game game)
        {
            string gameInfoJson = Serializers.Serialize(game.GameInfo);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                string playerJson = Serializers.Serialize(player, playerSerializationSettings);
                log.Info($"Sending PostStartGame to {player}");
                RpcId(player.NetworkId, nameof(GameStarted), gameInfoJson, playerJson, player.Num);
            }
        }

        [Remote]
        public void GameStarted(string gameInfoJson, string playerJson, int playerNum)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            // make sure our deserializer maps up our player correctly
            var player = Serializers.DeserializeObject<Player>(playerJson, Serializers.CreatePlayerSettings(TechStore.Instance));

            // notify any clients that we have a new game
            Client.EventManager.PublishGameStartedEvent(gameInfo, player);
            log.Info($"{player} Received PostStartGame");
        }

        #region Game Events

        public void SendTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"Sending TurnSubmitted for: {player}");
            Rpc(nameof(TurnSubmitted), Serializers.Serialize(gameInfo), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnSubmitted(string gameInfoJson, string playerJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"Received TurnSubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnSubmittedEvent(gameInfo, player);
            }
            else
            {
                log.Error($"Failed to parse json: {playerJson}");
            }
        }

        public void SendTurnUnsubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"Sending TurnUnsubmitted for: {player}");
            Rpc(nameof(TurnUnsubmitted), Serializers.Serialize(gameInfo), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnUnsubmitted(string gameInfoJson, string playerJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"Received TurnUnsubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnUnsubmittedEvent(gameInfo, player);
            }
            else
            {
                log.Error($"Failed to parse json: {playerJson}");
            }
        }


        /// <summary>
        /// Send a message to each client that the turn has passed a new turn is available. This will also
        /// update each player with their player data for the new turn.
        /// </summary>
        /// <param name="game">The game with players to notify</param>
        public void SendTurnPassed(Game game)
        {
            string gameInfoJson = Serializers.Serialize(game.GameInfo);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                string playerJson = Serializers.Serialize(player, playerSerializationSettings);
                log.Info($"Sending TurnPassed to {player}");
                RpcId(player.NetworkId, nameof(TurnPassed), gameInfoJson, playerJson, player.Num);
            }
        }

        [Remote]
        public void TurnPassed(string gameInfoJson, string playerJson, int playerNum)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            // make sure our deserializer maps up our player correctly
            var player = Serializers.DeserializeObject<Player>(playerJson, Serializers.CreatePlayerSettings(TechStore.Instance));

            Client.EventManager.PublishTurnPassedEvent(gameInfo, player);
            log.Info($"{player} Received TurnPassed");
        }

        public void SendTurnGenerating(PublicGameInfo gameInfo)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"Sending TurnGenerating to players");
            Rpc(nameof(TurnGenerating), gameInfoJson);
        }

        [Remote]
        public void TurnGenerating(string gameInfoJson)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishTurnGeneratingEvent();
            log.Info($"Received TurnGenerating");
        }

        public void SendTurnGeneratorAdvanced(PublicGameInfo gameInfo, TurnGenerationState state)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"Sending TurnGeneratorAdvanced to players");
            Rpc(nameof(TurnGeneratorAdvanced), gameInfoJson, state);
        }

        [Remote]
        public void TurnGeneratorAdvanced(string gameInfoJson, TurnGenerationState state)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishTurnGeneratorAdvancedEvent(state);
            log.Info($"Received TurnGeneratorAdvanced");
        }

        #endregion
    }
}
