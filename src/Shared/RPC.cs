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
        public delegate void PlayerRejoinedGameDelegate(string token, int playerNum, int networkId);

        // The RPC class recieves network messages and publishes events to it's local instance
        // This way, a server can listen for events only on the RPC instance in its scene tree
        public event Action<GameSettings<Player>> StartNewGameRequestedEvent;
        public event Action<string, int> ContinueGameRequestedEvent;
        public event PlayerRejoinedGameDelegate PlayerRejoinedGameEvent;
        public event Action<PublicPlayerInfo> PlayerDataRequestedEvent;
        public event Action<Player> SubmitTurnRequestedEvent;
        public event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;

        public event Action<PlayerMessage> PlayerMessageEvent;
        public event Action<Player, Race> RaceUpdatedEvent;
        public event Action AddAIPlayerEvent;
        public event Action<int> KickPlayerEvent;
        public event Action<PublicGameInfo, Player> ServerPlayerDataEvent;
        public event Action<List<PublicPlayerInfo>> ServerPlayersListEvent;
        public event Action<PublicPlayerInfo> PlayerJoinedNewGameEvent;
        public event Action<PublicGameInfo> PlayerJoinedExistingGameEvent;
        public event Action<PublicPlayerInfo, List<PublicPlayerInfo>> PlayerLeftEvent;

        #endregion

        /// <summary>
        /// The server side RPC needs an active list of players for deserializing player
        /// json for turn submittals
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// We have a uniqe RPC instance per scene tree
        /// </summary>
        public static RPC Instance(SceneTree sceneTree)
        {
            return sceneTree.Root.GetNode<RPC>("/root/RPC");
        }

        #region Player RPC Calls

        public void SendMessage(string message, int playerNum = PlayerMessage.ServerPlayerNum)
        {
            var playerMessage = new PlayerMessage(playerNum, message);
            log.Debug($"{LogPrefix} Sending Message {playerMessage}");
            Rpc(nameof(Message), Serializers.Serialize(playerMessage));
        }

        /// <summary>
        /// Send a client 
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="messages"></param>
        public void ServerSendClientAllMessages(int networkId, List<PlayerMessage> messages)
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

        /// <summary>
        /// Send a message to all clients that a new player joined
        /// </summary>
        /// <param name="player"></param>
        public void ServerSendClientPlayerJoinedNewGame(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying clients about player join: {player}");
            var playerJson = Serializers.Serialize(player);
            Rpc(nameof(PlayerJoinedNewGame), playerJson);
        }

        [Remote]
        public void PlayerJoinedNewGame(string playerJson)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerJoined event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerJoinedNewGameEvent?.Invoke(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {playerJson}");
            }
        }

        /// <summary>
        /// If a network game is in progress, send the newly joined player the gameInfo so they
        /// can resume playing a player
        /// </summary>
        /// <param name="player"></param>
        public void ServerSendClientPlayerJoinedExistingGame(int networkId, PublicGameInfo gameInfo)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying new player about game: {gameInfo}");
            var gameInfoJson = Serializers.Serialize(gameInfo);
            RpcId(networkId, nameof(PlayerJoinedExistingGame), gameInfoJson);
        }

        [Remote]
        public void PlayerJoinedExistingGame(string gameInfoJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            if (gameInfo != null)
            {
                log.Debug($"{LogPrefix} Received PlayerJoinedExistingGame event from server.");

                // notify listeners that we have updated Player
                PlayerJoinedExistingGameEvent?.Invoke(gameInfo);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {gameInfoJson}");
            }
        }

        /// <summary>
        /// Send a message to all clients that a player left
        /// </summary>
        /// <param name="player"></param>
        public void ServerSendClientPlayerLeft(PublicPlayerInfo player, List<PublicPlayerInfo> remainingPlayers)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Notifying clients about player left: {player}");
            Rpc(nameof(PlayerLeft), Serializers.Serialize(player), Serializers.Serialize(remainingPlayers));
        }

        [Remote]
        public void PlayerLeft(string playerJson, string remainingPlayersJson)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            var remainingPlayers = Serializers.DeserializeObject<List<PublicPlayerInfo>>(remainingPlayersJson);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerLeft event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                // notify listeners that we have updated Player
                PlayerLeftEvent?.Invoke(player, remainingPlayers);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {playerJson}");
            }
        }

        public void ClientSendServerPlayerUpdated(PublicPlayerInfo player)
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

                if (this.IsServer())
                {
                    if (player.Num >= 0 && player.Num < Players.Count && Players[player.Num].NetworkId == GetTree().GetRpcSenderId())
                    {
                        log.Info($"{LogPrefix} Updating server side player: {player}");
                        Players[player.Num].Update(player);
                    }
                    else
                    {
                        log.Error($"{LogPrefix} Received Invalid PlayerUpdated event for Player {player} from {GetTree().GetRpcSenderId()}");
                    }
                }
                // notify listeners that we have updated Player
                NetworkClient.Instance.PublishPlayerUpdatedEvent(player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {json}");
            }
        }

        public void ClientSendServerPlayerDataRequest(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Asking server for a player data update: {player}");
            Rpc(nameof(PlayerDataRequested), Serializers.Serialize(player));
        }

        [Remote]
        public void PlayerDataRequested(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received PlayerDataRequested event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                if (this.IsServer())
                {
                    if (player.Num >= 0 && player.Num < Players.Count) // && Players[player.Num].NetworkId == GetTree().GetRpcSenderId())
                    {
                        log.Info($"{LogPrefix} Sending server side player to player: {player}");
                        PlayerDataRequestedEvent?.Invoke(player);
                    }
                    else
                    {
                        log.Error($"{LogPrefix} Received Invalid PlayerDataRequested event for Player {player} from {GetTree().GetRpcSenderId()}");
                    }
                }
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
        public void ServerSendClientPlayersList(List<PublicPlayerInfo> players, int networkId)
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
        public void ClientSendServerAddAIPlayer()
        {

            log.Info($"{LogPrefix}: Sending AddAIPlayer to server");
            RpcId(1, nameof(AddAIPlayer));
        }

        [Remote]
        public void AddAIPlayer()
        {
            if (this.IsServer())
            {
                // make sure the sending player is the host
                var callerNetworkId = GetTree().GetRpcSenderId();
                var player = Players.Find(player => player.NetworkId == callerNetworkId);
                if (player == null || player.Host == false)
                {
                    log.Error($"{LogPrefix}: Received AddAIPlayer from {callerNetworkId}, player doesn't exist or isn't host.");
                }
                else
                {
                    log.Info($"{LogPrefix}: Received AddAIPlayer from {player}");
                    AddAIPlayerEvent?.Invoke();
                }
            }
            else
            {
                log.Error("A client received a AddAIPlayer RPC call");
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void ClientSendServerKickPlayer(int playerNum)
        {

            log.Info($"{LogPrefix}: Sending KickPlayer to server");
            RpcId(1, nameof(KickPlayer), playerNum);
        }

        [Remote]
        public void KickPlayer(int playerNum)
        {
            if (this.IsServer())
            {
                // make sure the sending player is the host
                var callerNetworkId = GetTree().GetRpcSenderId();
                var player = Players.Find(player => player.NetworkId == callerNetworkId);
                if (player == null || player.Host == false)
                {
                    log.Error($"{LogPrefix}: Received KickPlayer from {callerNetworkId}, player doesn't exist or isn't host.");
                }
                else
                {
                    log.Info($"{LogPrefix}: Received KickPlayer from {player} to kick {playerNum}");
                    KickPlayerEvent?.Invoke(playerNum);
                }
            }
            else
            {
                log.Error("A client received a KickPlayer RPC call");
            }
        }


        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void ClientSendServerStartNewGameRequested(GameSettings<Player> settings)
        {

            log.Info($"{LogPrefix}: Sending GameStartRequested to server");
            var settingsJson = Serializers.Serialize<GameSettings<Player>>(settings);
            RpcId(1, nameof(StartNewGameRequested), settingsJson);
        }

        [Remote]
        public void StartNewGameRequested(string settingsJson)
        {
            if (this.IsServer())
            {
                // deserialize the new game settings and set our list of network players to the Players property
                GameSettings<Player> settings = Serializers.DeserializeObject<GameSettings<Player>>(settingsJson);
                settings.Players = new(Players);

                // make sure the sending player is the host
                var callerNetworkId = GetTree().GetRpcSenderId();
                var player = Players.Find(player => player.NetworkId == callerNetworkId);
                if (player == null || player.Host == false)
                {
                    log.Error($"{LogPrefix}: Received GameStartRequested from {callerNetworkId}, player doesn't exist or isn't host.");
                }
                else
                {
                    log.Info($"{LogPrefix}: Received GameStartRequested from {player}");
                    StartNewGameRequestedEvent?.Invoke(settings);
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
        public void ClientSendServerPlayerRejoinedGame(Player player)
        {
            log.Info($"{LogPrefix}: Sending PlayerRejoinedGame to server");
            RpcId(1, nameof(PlayerRejoinedGame), player.Token, player.Num);
        }

        [Remote]
        public void PlayerRejoinedGame(string token, int playerNum)
        {
            if (this.IsServer())
            {
                log.Info($"{LogPrefix}: Received PlayerRejoinedGame from player {playerNum} - networkId: {GetTree().GetRpcSenderId()}");
                PlayerRejoinedGameEvent?.Invoke(token, playerNum, GetTree().GetRpcSenderId());
            }
            else
            {
                log.Error("A client received a PlayerRejoinedGame RPC call");
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void ClientSendServerContinueGameRequested(string gameName, int year)
        {
            log.Info($"{LogPrefix}: Sending ContinueGameRequested to server");
            RpcId(1, nameof(ContinueGameRequested), gameName, year);
        }

        [Remote]
        public void ContinueGameRequested(string gameName, int year)
        {
            if (this.IsServer())
            {
                log.Info($"{LogPrefix}: Received GameStartRequested from networkId: {GetTree().GetRpcSenderId()}");
                ContinueGameRequestedEvent?.Invoke(gameName, year);
            }
            else
            {
                log.Error("A client received a ContinueGameRequested RPC call");
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void ServerSendClientPlayerData(PublicGameInfo gameInfo, Player player)
        {
            var players = gameInfo != null ? gameInfo.Players : new List<PublicPlayerInfo>() { player };
            var settings = Serializers.CreatePlayerSettings(players, TechStore.Instance);

            log.Info($"Server: Sending {player} updated player data");
            string json = Serializers.Serialize(player, settings);
            string gameInfoJson = gameInfo != null ? Serializers.Serialize(gameInfo) : null;

            // send this player their data, unless this is an AI player, then send it to the first player (the host)
            // so that player can configure the AI player
            var networkId = player.NetworkId;
            if (networkId == 0)
            {
                networkId = Players[0].NetworkId;
            }

            RpcId(networkId, nameof(ServerPlayerData), gameInfoJson, player.Num, json);
        }

        [Remote]
        public void ServerPlayerData(string gameInfoJson, int playerNum, string playerJson)
        {
            log.Info("Client: Server sent player data");
            PublicGameInfo gameInfo = gameInfoJson != null ? Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson) : null;
            var player = new Player() { Num = playerNum };
            var players = gameInfo != null ? gameInfo.Players : new List<PublicPlayerInfo>() { player };

            Serializers.PopulatePlayer(playerJson, player, Serializers.CreatePlayerSettings(players, TechStore.Instance));
            log.Info("Client: Done recieving updated player data");

            // notify any listeners that we have new data for this player
            ServerPlayerDataEvent?.Invoke(gameInfo, player);
            Client.EventManager.PublishPlayerDataEvent(gameInfo, player);
        }

        /// <summary>
        /// This is called by clients to submit turns to the server
        /// </summary>
        /// <param name="player">The player (probably PlayersManager.Me) submitting the turn</param>
        public void ClientSendServerSubmitTurn(Player player)
        {
            var settings = Serializers.CreatePlayerSettings(player.Game.Players, player.TechStore);
            log.Info($"Client: Submitting turn to server");
            var playerJson = Serializers.Serialize(player, settings);
            RpcId(1, nameof(TurnSubmitRequested), player.Token, Serializers.Serialize(player.Game), playerJson);
        }

        [Remote]
        public void TurnSubmitRequested(string token, string gameInfoJson, string playerJson)
        {
            // find the network player by this number
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Players.Find(player => player.Token == token);
            if (player != null)
            {
                // load the actual player from JSON
                log.Info($"Server: {player} submitted turn");
                player = new Player() { Num = player.Num };
                Serializers.PopulatePlayer(playerJson, player, Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance));

                // submit this player's turn to the server
                SubmitTurnRequestedEvent?.Invoke(player);
            }
            else
            {
                log.Error($"Player for token: {token} not found");
            }

        }

        /// <summary>
        /// This is called by clients to submit turns to the server
        /// </summary>
        /// <param name="player">The player (probably PlayersManager.Me) submitting the turn</param>
        public void ClientSendServerUpdateRace(Race race)
        {
            log.Info($"Client: Sending race to server");
            var json = Serializers.Serialize(race);
            RpcId(1, nameof(RaceUpdated), json);
        }

        [Remote]
        public void RaceUpdated(string raceJson)
        {
            // find the network player by this number
            var networkId = GetTree().GetRpcSenderId();
            var player = Players.Find(player => player.NetworkId == networkId);
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

        public void ServerSendClientGameStarting(PublicGameInfo gameInfo)
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
        public void ServerSendClientGameStarted(Game game)
        {
            string gameInfoJson = Serializers.Serialize(game.GameInfo);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.GameInfo.Players, game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                string playerJson = Serializers.Serialize(player, playerSerializationSettings);
                log.Info($"{LogPrefix}: Sending PostStartGame to {player}");
                RpcId(player.NetworkId, nameof(GameStarted), gameInfoJson, playerJson, player.Num);
            }
        }

        [Remote]
        public void GameStarted(string gameInfoJson, string playerJson, int playerNum)
        {
            PublicGameInfo gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            // make sure our deserializer maps up our player correctly
            var player = new Player() { Num = playerNum };
            var playerSerializationSettings = Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance);
            Serializers.PopulatePlayer(playerJson, player, playerSerializationSettings);

            // notify any clients that we have a new game
            Client.EventManager.PublishGameStartedEvent(gameInfo, player);
            log.Info($"{LogPrefix}:{player} Received PostStartGame");
        }

        #region Game Events

        public void ServerSendClientTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Sending TurnSubmitted for: {player}");
            Rpc(nameof(TurnSubmitted), Serializers.Serialize(gameInfo), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnSubmitted(string gameInfoJson, string playerJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received TurnSubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnSubmittedEvent(gameInfo, player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {playerJson}");
            }
        }

        public void ServerSendClientTurnUnsubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{LogPrefix} Sending TurnUnsubmitted for: {player}");
            Rpc(nameof(TurnUnsubmitted), Serializers.Serialize(gameInfo), Serializers.Serialize(player));
        }

        [Remote]
        public void TurnUnsubmitted(string gameInfoJson, string playerJson)
        {
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(playerJson);
            if (player != null)
            {
                log.Debug($"{LogPrefix} Received TurnUnsubmitted event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");
                Client.EventManager.PublishTurnUnsubmittedEvent(gameInfo, player);
            }
            else
            {
                log.Error($"{LogPrefix} Failed to parse json: {playerJson}");
            }
        }


        /// <summary>
        /// Send a message to each client that the turn has passed a new turn is available. This will also
        /// update each player with their player data for the new turn.
        /// </summary>
        /// <param name="game">The game with players to notify</param>
        public void ServerSendClientTurnPassed(Game game)
        {
            string gameInfoJson = Serializers.Serialize(game.GameInfo);
            var playerSerializationSettings = Serializers.CreatePlayerSettings(game.GameInfo.Players, game.TechStore);
            foreach (var player in game.Players.Where(player => !player.AIControlled && player.NetworkId != 0))
            {
                string playerJson = Serializers.Serialize(player, playerSerializationSettings);
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
            var playerSerializationSettings = Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance);
            Serializers.PopulatePlayer(playerJson, player, playerSerializationSettings);

            Client.EventManager.PublishTurnPassedEvent(gameInfo, player);
            log.Info($"{LogPrefix}:{player} Received TurnPassed");
        }

        public void ServerSendClientTurnGenerating(PublicGameInfo gameInfo)
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

        public void ServerSendClientTurnGeneratorAdvanced(PublicGameInfo gameInfo, TurnGenerationState state)
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
