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
    /// The RPC node is at the root of the client and server SceneTrees. This is how a
    /// client or server makes RPC calls back and forth to each other.
    /// </summary>
    public class RPC : Node, IClientEventPublisher
    {
        static CSLog log = LogProvider.GetLogger(typeof(RPC));

        string LogPrefix { get => this.IsServer() ? "Server:" : "Client:"; }

        #region RPC events

        // The RPC class recieves network messages and publishes events to it's local instance
        // This way, a server can listen for events only on the RPC instance in its scene tree
        public event Action<GameSettings<Player>> GameStartRequestedEvent;
        public event Action<Player> SubmitTurnRequestedEvent;
        public event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;

        public event Action<PlayerMessage> PlayerMessageEvent;
        public event Action<Player, Race> RaceUpdatedEvent;
        public event Action<Player> ServerPlayerDataEvent;
        public event Action<List<PublicPlayerInfo>> ServerPlayersListEvent;
        public event Action<PublicPlayerInfo> PlayerJoinedEvent;
        public event Action<PublicPlayerInfo> PlayerLeftEvent;

        #endregion

        /// <summary>
        /// The server side RPC needs an active list of players for deserializing player
        /// json for turn submittals
        /// </summary>
        List<Player> players;

        /// <summary>
        /// We have a uniqe RPC instance per scene tree
        /// </summary>
        public static RPC Instance(SceneTree sceneTree)
        {
            return sceneTree.Root.GetNode<RPC>("/root/RPC");
        }

        public void SetPlayers(List<Player> players)
        {
            this.players = players;
        }

        #region Player RPC Calls

        public void SendMessage(string message, int playerNum = PlayerMessage.ServerPlayerNum)
        {
            var playerMessage = new PlayerMessage(playerNum, message);
            log.Debug($"{LogPrefix} Sending Message {playerMessage}");
            Rpc(nameof(Message), Serializers.Serialize(playerMessage));
        }

        public void SendAllMessages(int networkId, List<PlayerMessage> messages)
        {
            log.Debug($"{LogPrefix} Sending All Messages to {networkId}");
            messages.ForEach(m => RpcId(networkId, nameof(Message), Serializers.Serialize(m)));
        }

        [RemoteSync]
        public void Message(string json)
        {
            var message = Serializers.Deserialize<PlayerMessage>(json);
            if (message.HasValue)
            {
                log.Debug($"{LogPrefix} Received PlayerMessage {message} from {GetTree().GetRpcSenderId()}");

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
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        public void SendPlayerJoined(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying clients about player join: {player}");
            Rpc(nameof(PlayerJoined), Serializers.Serialize(player));
        }

        [Remote]
        public void PlayerJoined(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerJoined event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerJoinedEvent?.Invoke(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        public void SendPlayerLeft(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying clients about player left: {player}");
            Rpc(nameof(PlayerLeft), Serializers.Serialize(player));
        }

        [Remote]
        public void PlayerLeft(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerLeft event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerLeftEvent?.Invoke(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        public void SendPlayerUpdated(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying clients about player update: {player}");
            Rpc(nameof(PlayerUpdated), Serializers.Serialize(player));
        }

        [Remote]
        public void PlayerUpdated(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                NetworkClient.Instance.PublishPlayerUpdatedEvent(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        /// <summary>
        /// Called by the server to notify all clients about new player data
        /// </summary>
        /// <param name="players"></param>
        /// <param name="networkId"></param>
        public void SendClientPlayersList(List<PublicPlayerInfo> players, int networkId)
        {
            // servers listen for signals and notify clients
            if (this.IsServer())
            {
                var playersArray = new Godot.Collections.Array(players.Select(p => Serializers.Serialize(p)).ToArray());
                log.Debug($"{LogPrefix} Sending player infos to network player: {networkId}");
                // we are a server, tell the clients we have a player update
                RpcId(networkId, nameof(ServerPlayersList), playersArray);
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
        public void ServerPlayersList(Godot.Collections.Array jsons)
        {
            var players = new List<PublicPlayerInfo>(jsons.Count);
            foreach (string json in jsons)
            {
                var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
                if (player != null)
                {
                    log.Debug($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                    players.Add(player);
                }
                else
                {
                    log.Error($"{LogPrefix} Failed to parse json: {json}");
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
        public void SendGameStartRequested(GameSettings<Player> settings)
        {

            log.Info($"{LogPrefix}: Sending GameStartRequested to server");
            var settingsJson = Serializers.Serialize<GameSettings<Player>>(settings);
            RpcId(1, nameof(GameStartRequested), settingsJson);
        }

        [Remote]
        public void GameStartRequested(string settingsJson)
        {
            if (this.IsServer())
            {
                // deserialize the new game settings and set our list of network players to the Players property
                GameSettings<Player> settings = Serializers.DeserializeObject<GameSettings<Player>>(settingsJson);
                settings.Players = new(players);

                // make sure the sending player is the host
                var callerNetworkId = GetTree().GetRpcSenderId();
                var player = players.Find(player => player.NetworkId == callerNetworkId);
                if (player == null || player.Host == false)
                {
                    log.Error($"{LogPrefix}: Received GameStartRequested from {callerNetworkId}, player doesn't exist or isn't host.");
                }
                else
                {
                    log.Info($"{LogPrefix}: Received GameStartRequested from {player}");
                    GameStartRequestedEvent?.Invoke(settings);
                }
            }
            else
            {
                log.Error("A client received a GameStartRequested RPC call");
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendClientPlayerData(Player player)
        {
            var settings = Serializers.CreatePlayerSettings(new List<PublicPlayerInfo>() { player }, TechStore.Instance);

            log.Info($"Server: Sending {player} updated player data");
            string json = Serializers.Serialize(player, settings);
            RpcId(player.NetworkId, nameof(ServerPlayerData), player.Num, json);
        }

        [Remote]
        public void ServerPlayerData(int playerNum, string playerJson)
        {
            log.Info("Client: Server sent player data, updated Me");
            var player = new Player() { Num = playerNum };

            Serializers.PopulatePlayer(playerJson, player, Serializers.CreatePlayerSettings(new List<PublicPlayerInfo>() { player }, TechStore.Instance));
            player.SetupMapObjectMappings();
            player.ComputeAggregates();
            log.Info("Client: Done recieving updated player data");

            // notify any listeners that we have new data for this player
            ServerPlayerDataEvent?.Invoke(player);
        }

        /// <summary>
        /// This is called by clients to submit turns to the server
        /// </summary>
        /// <param name="player">The player (probably PlayersManager.Me) submitting the turn</param>
        public void SendSubmitTurn(Player player)
        {
            var settings = Serializers.CreatePlayerSettings(player.Game.Players, player.TechStore);
            log.Info($"Client: Submitting turn to server");
            var json = Serializers.Serialize(player, settings);
            RpcId(1, nameof(SubmitTurn), json);
        }

        [Remote]
        public void SubmitTurn(string playerJson)
        {
            // find the network player by this number
            var networkId = GetTree().GetRpcSenderId();
            var player = players.Find(player => player.NetworkId == networkId);
            if (player != null)
            {
                // load the actual player from JSON
                log.Info($"Server: {player} submitted turn");
                player = new Player() { Num = player.Num };
                Serializers.PopulatePlayer(playerJson, player, Serializers.CreatePlayerSettings(players.Cast<PublicPlayerInfo>().ToList(), TechStore.Instance));

                // submit this player's turn to the server
                SubmitTurnRequestedEvent?.Invoke(player);
            }
            else
            {
                log.Error($"Player for networkId: {networkId} not found");
            }

        }

        /// <summary>
        /// This is called by clients to submit turns to the server
        /// </summary>
        /// <param name="player">The player (probably PlayersManager.Me) submitting the turn</param>
        public void SendUpdateRace(Race race)
        {
            log.Info($"Client: Sending race to server");
            var json = Serializers.Serialize(race);
            RpcId(1, nameof(UpdateRace), json);
        }

        [Remote]
        public void UpdateRace(string raceJson)
        {
            // find the network player by this number
            var networkId = GetTree().GetRpcSenderId();
            var player = players.Find(player => player.NetworkId == networkId);
            if (player != null)
            {
                // load the actual player from JSON
                log.Info($"Server: {player} sent race");
                var race = Serializers.DeserializeObject<Race>(raceJson);
                player.Race = race;

                // submit this player's turn to the server
                RaceUpdatedEvent?.Invoke(player, race);
            }
            else
            {
                log.Error($"Player for networkId: {networkId} not found");
            }

        }        

        public void SendGameStarting(PublicGameInfo gameInfo)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"{LogPrefix}: Sending GameStarting to players");
            Rpc(nameof(GameStarting), gameInfoJson);
        }

        [Remote]
        public void GameStarting(string gameInfoJson)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishGameStartingEvent(gameInfo);
            log.Info($"{LogPrefix} Received GameStarting");
        }

        /// <summary>
        /// Sent by the server to notify players the game is started
        /// </summary>
        /// <param name="year"></param>
        /// <param name="networkId"></param>
        public void SendPostStartGame(Game game)
        {
            string gameInfoJson = Serializers.Serialize(game.GameInfo);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.GameInfo.Players, game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                String playerJson = Serializers.Serialize(player, playerSerializationSettings);
                log.Info($"{LogPrefix}: Sending PostStartGame to {player}");
                RpcId(player.NetworkId, nameof(PostStartGame), gameInfoJson, playerJson, player.Num);
            }
        }

        [Remote]
        public void PostStartGame(string gameInfoJson, string playerJson, int playerNum)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            // make sure our deserializer maps up our player correctly
            var player = new Player() { Num = playerNum };
            gameInfo.Players.RemoveAt(player.Num);
            gameInfo.Players.Insert(player.Num, player);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance);
            Serializers.PopulatePlayer(playerJson, player, playerSerializationSettings);
            // player.SetupMapObjectMappings();
            // player.ComputeAggregates();

            // notify any clients that we have a new game
            Client.EventManager.PublishGameStartedEvent(gameInfo, player);
            log.Info($"{LogPrefix}:{player} Received PostStartGame");
        }

        #region Game Events

        public void SendTurnSubmitted(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Sending TurnSubmitted for: {player}");
            Rpc(nameof(TurnSubmitted), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnSubmitted(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received TurnSubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnSubmittedEvent(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        public void SendTurnUnsubmitted(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Sending TurnUnsubmitted for: {player}");
            Rpc(nameof(TurnUnsubmitted), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnUnsubmitted(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received TurnUnsubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnUnsubmittedEvent(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
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
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.GameInfo.Players, game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                String playerJson = Serializers.Serialize(player, playerSerializationSettings);
                log.Info($"{LogPrefix}: Sending TurnPassed to {player}");
                RpcId(player.NetworkId, nameof(TurnPassed), gameInfoJson, playerJson, player.Num);
            }
        }

        [Remote]
        public void TurnPassed(string gameInfoJson, string playerJson, int playerNum)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            // make sure our deserializer maps up our player correctly
            var player = new Player() { Num = playerNum };
            gameInfo.Players.RemoveAt(player.Num);
            gameInfo.Players.Insert(player.Num, player);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance);
            Serializers.PopulatePlayer(playerJson, player, playerSerializationSettings);
            // player.SetupMapObjectMappings();
            // player.ComputeAggregates();

            Client.EventManager.PublishTurnPassedEvent(gameInfo, player);
            log.Info($"{LogPrefix}:{player} Received TurnPassed");
        }

        public void SendTurnGenerating(PublicGameInfo gameInfo)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"{LogPrefix}: Sending TurnGenerating to players");
            Rpc(nameof(TurnGenerating), gameInfoJson);
        }

        [Remote]
        public void TurnGenerating(string gameInfoJson)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishTurnGeneratingEvent();
            log.Info($"{LogPrefix} Received TurnGenerating");
        }

        public void SendTurnGeneratorAdvanced(PublicGameInfo gameInfo, TurnGenerationState state)
        {
            string gameInfoJson = Serializers.Serialize(gameInfo);
            log.Info($"{LogPrefix}: Sending TurnGeneratorAdvanced to players");
            Rpc(nameof(TurnGeneratorAdvanced), gameInfoJson, state);
        }

        [Remote]
        public void TurnGeneratorAdvanced(string gameInfoJson, TurnGenerationState state)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            Client.EventManager.PublishTurnGeneratorAdvancedEvent(state);
            log.Info($"{LogPrefix} Received TurnGeneratorAdvanced");
        }

        #endregion
    }
}
