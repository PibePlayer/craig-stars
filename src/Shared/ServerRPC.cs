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
    /// The ServerRPC also publishes events to the server so the server can respond to these RPC calls
    /// </summary>
    public class ServerRPC : Node, IClientEventPublisher
    {
        static CSLog log = LogProvider.GetLogger(typeof(ServerRPC));

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

        public event Action<Player, Race> RaceUpdatedEvent;
        public event Action AddAIPlayerEvent;
        public event Action<int> KickPlayerEvent;

        #endregion

        /// <summary>
        /// The server side RPC needs an active list of players for deserializing player
        /// json for turn submittals
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// We have a uniqe RPC instance per scene tree
        /// </summary>
        public static ServerRPC Instance(SceneTree sceneTree)
        {
            return sceneTree.Root.GetNode<ServerRPC>("/root/ServerRPC");
        }

        #region Broadcasts to All Network Peers

        /// <summary>
        /// Update the server (and other clients) with information about this player, like color or name
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerUpdated(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"{(this.IsServer() ? "Server:" : "Client:")} Notifying clients about player update: {player}");
            Rpc(nameof(PlayerUpdated), Serializers.Serialize(player));
        }

        [Remote]
        void PlayerUpdated(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                if (this.IsServer())
                {
                    // servers update the player data on the server
                    if (player.Num >= 0 && player.Num < Players.Count && Players[player.Num].NetworkId == GetTree().GetRpcSenderId())
                    {
                        log.Info($"Updating server side player: {player}");
                        Players[player.Num].Update(player);
                    }
                    else
                    {
                        log.Error($"Received Invalid PlayerUpdated event for Player {player} from {GetTree().GetRpcSenderId()}");
                    }
                }
                else
                {
                    // clients notify listeners
                    NetworkClient.Instance.PublishPlayerUpdatedEvent(player);
                }
            }
            else
            {
                log.Error($"Failed to parse json: {json}");
            }
        }

        #endregion


        /// <summary>
        /// Send a request for updated player data
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerDataRequest(PublicPlayerInfo player)
        {
            // send our peers an update of a player
            log.Debug($"Asking server for a player data update: {player}");
            RpcId(1, nameof(PlayerDataRequested), Serializers.Serialize(player));
        }

        [Remote]
        void PlayerDataRequested(string json)
        {
            var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
            if (player != null)
            {
                log.Debug($"Received PlayerDataRequested event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                if (player.Num >= 0 && player.Num < Players.Count) // && Players[player.Num].NetworkId == GetTree().GetRpcSenderId())
                {
                    log.Info($"Sending server side player to player: {player}");
                    PlayerDataRequestedEvent?.Invoke(player);
                }
                else
                {
                    log.Error($"Received Invalid PlayerDataRequested event for Player {player} from {GetTree().GetRpcSenderId()}");
                }
            }
            else
            {
                log.Error($"Failed to parse json: {json}");
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendAddAIPlayer()
        {

            log.Info($"Sending AddAIPlayer to server");
            RpcId(1, nameof(AddAIPlayer));
        }

        [Remote]
        void AddAIPlayer()
        {
            if (this.IsServer())
            {
                // make sure the sending player is the host
                var callerNetworkId = GetTree().GetRpcSenderId();
                var player = Players.Find(player => player.NetworkId == callerNetworkId);
                if (player == null || player.Host == false)
                {
                    log.Error($"Received AddAIPlayer from {callerNetworkId}, player doesn't exist or isn't host.");
                }
                else
                {
                    log.Info($"Received AddAIPlayer from {player}");
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
        public void SendKickPlayer(int playerNum)
        {

            log.Info($"Sending KickPlayer to server");
            RpcId(1, nameof(KickPlayer), playerNum);
        }

        [Remote]
        void KickPlayer(int playerNum)
        {
            // make sure the sending player is the host
            var callerNetworkId = GetTree().GetRpcSenderId();
            var player = Players.Find(player => player.NetworkId == callerNetworkId);
            if (player == null || player.Host == false)
            {
                log.Error($"Received KickPlayer from {callerNetworkId}, player doesn't exist or isn't host.");
            }
            else
            {
                log.Info($"Received KickPlayer from {player} to kick {playerNum}");
                KickPlayerEvent?.Invoke(playerNum);
            }
        }


        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendStartNewGameRequest(GameSettings<Player> settings)
        {

            log.Info($"Sending GameStartRequested to server");
            var settingsJson = Serializers.Serialize<GameSettings<Player>>(settings);
            RpcId(1, nameof(StartNewGameRequested), settingsJson);
        }

        [Remote]
        void StartNewGameRequested(string settingsJson)
        {
            // deserialize the new game settings and set our list of network players to the Players property
            GameSettings<Player> settings = Serializers.DeserializeObject<GameSettings<Player>>(settingsJson);
            settings.Players = new(Players);

            // make sure the sending player is the host
            var callerNetworkId = GetTree().GetRpcSenderId();
            var player = Players.Find(player => player.NetworkId == callerNetworkId);
            if (player == null || player.Host == false)
            {
                log.Error($"Received GameStartRequested from {callerNetworkId}, player doesn't exist or isn't host.");
            }
            else
            {
                log.Info($"Received GameStartRequested from {player}");
                StartNewGameRequestedEvent?.Invoke(settings);
            }
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendPlayerRejoinedGame(Player player)
        {
            log.Info($"Sending PlayerRejoinedGame to server");
            RpcId(1, nameof(PlayerRejoinedGame), player.Token, player.Num);
        }

        [Remote]
        void PlayerRejoinedGame(string token, int playerNum)
        {
            log.Info($"Received PlayerRejoinedGame from player {playerNum} - networkId: {GetTree().GetRpcSenderId()}");
            PlayerRejoinedGameEvent?.Invoke(token, playerNum, GetTree().GetRpcSenderId());
        }

        /// <summary>
        /// This is called by the server to send each player their player information
        /// </summary>
        /// <param name="player"></param>
        public void SendContinueGameRequested(string gameName, int year)
        {
            log.Info($"Sending ContinueGameRequested to server");
            RpcId(1, nameof(ContinueGameRequested), gameName, year);
        }

        [Remote]
        void ContinueGameRequested(string gameName, int year)
        {
            log.Info($"Received GameStartRequested from networkId: {GetTree().GetRpcSenderId()}");
            ContinueGameRequestedEvent?.Invoke(gameName, year);
        }

        /// <summary>
        /// This is called by clients to submit turns to the server
        /// </summary>
        /// <param name="player">The player (probably PlayersManager.Me) submitting the turn</param>
        public void SendSubmitTurn(PublicGameInfo gameInfo, Player player)
        {
            var settings = Serializers.CreatePlayerSettings(TechStore.Instance);
            log.Info($"Client: Submitting turn to server");
            var playerJson = Serializers.Serialize(player, settings);
            RpcId(1, nameof(TurnSubmitRequested), player.Token, Serializers.Serialize(gameInfo), playerJson);
        }

        [Remote]
        void TurnSubmitRequested(string token, string gameInfoJson, string playerJson)
        {
            // find the network player by this number
            var gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            var player = Players.Find(player => player.Token == token);
            if (player != null)
            {
                // load the actual player from JSON
                log.Info($"Server: {player} submitted turn");
                player = Serializers.DeserializeObject<Player>(playerJson, Serializers.CreatePlayerSettings(TechStore.Instance));

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
        public void SendUpdateRace(Race race)
        {
            log.Info($"Client: Sending race to server");
            var json = Serializers.Serialize(race);
            RpcId(1, nameof(RaceUpdated), json);
        }

        [Remote]
        void RaceUpdated(string raceJson)
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

    }
}
