using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;
using log4net;

namespace CraigStars.Singletons
{
    public class RPC : Node
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RPC));

        private String LogPrefix { get => this.IsServer() ? "Server:" : "Client:"; }

        /// <summary>
        /// RPC is a singleton
        /// </summary>
        private static RPC instance;
        public static RPC Instance
        {
            get
            {
                return instance;
            }
        }

        RPC()
        {
            instance = this;
        }

        public override void _Ready()
        {
            // when a player is updated, notify other players
            Signals.PlayerJoinedEvent += OnPlayerJoined;
        }

        public override void _ExitTree()
        {
            Signals.PlayerJoinedEvent -= OnPlayerJoined;
        }

        private void OnPlayerJoined(int networkId)
        {
            if (this.IsServer() && networkId != 1)
            {
                // tell the new player about our players
                SendPlayersUpdated(PlayersManager.Instance.Players, networkId);
            }
        }

        #region Connection RPC Calls

        #endregion

        #region Player RPC Calls

        public void SendMessage(string message)
        {
            var playerMessage = new PlayerMessage(PlayersManager.Instance.Me.Num, message);
            log.Debug($"{LogPrefix} Sending Message {playerMessage}");
            Rpc(nameof(Message), Serializers.Serialize(playerMessage));
        }

        public void SendAllMessages(int networkId)
        {
            log.Debug($"{LogPrefix} Sending All Messages to {networkId}");
            PlayersManager.Instance.Messages.ForEach(m => RpcId(networkId, nameof(Message), Serializers.Serialize(m)));
        }

        [RemoteSync]
        public void Message(string json)
        {
            var message = Serializers.Deserialize<PlayerMessage>(json);
            if (message.HasValue)
            {
                log.Debug($"{LogPrefix} Received PlayerMessage {message} from {GetTree().GetRpcSenderId()}");

                // notify listeners that we have updated Player
                Signals.PublishPlayerMessageEvent(message.Value);
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
                Signals.PublishPlayerUpdatedEvent(player);
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
        public void SendPlayersUpdated(List<PublicPlayerInfo> players, int networkId = 0)
        {
            // servers listen for signals and notify clients
            if (this.IsServer())
            {
                var playersArray = new Godot.Collections.Array(players.Select(p => Serializers.Serialize(p)).ToArray());
                if (networkId == 0)
                {
                    // log.Debug($"{LogPrefix} Sending all players to all clients");
                    // we are a server, tell the clients we have a player update
                    Rpc(nameof(PlayersUpdated), playersArray);
                }
                else
                {
                    log.Debug($"{LogPrefix} Sending players to {networkId}");
                    // we are a server, tell the clients we have a player update
                    RpcId(networkId, nameof(PlayersUpdated), playersArray);
                }
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
        public void PlayersUpdated(Godot.Collections.Array jsons)
        {
            var players = new PublicPlayerInfo[jsons.Count];
            foreach (string json in jsons)
            {
                var player = Serializers.DeserializeObject<PublicPlayerInfo>(json);
                if (player != null)
                {
                    log.Debug($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                    // notify listeners that we have updated Player
                    Signals.PublishPlayerUpdatedEvent(player);
                }
                else
                {
                    log.Error($"{LogPrefix} Failed to parse json: {json}");
                }
            }
        }

        #endregion

        /// <summary>
        /// Sent by the server to notify players the game is started
        /// </summary>
        /// <param name="year"></param>
        /// <param name="networkId"></param>
        public void SendPostStartGame(int year, int networkId = 0)
        {
            if (networkId == 0)
            {
                Rpc(nameof(PostStartGame), year);
            }
            else
            {
                RpcId(networkId, nameof(PostStartGame), year);
            }
        }

        [RemoteSync]
        public void PostStartGame(int year)
        {
            Signals.PublishPostStartGameEvent(year);
        }

        #region Game Events

        public void SendTurnPassed(int day)
        {
            Rpc(nameof(TurnPassed), day);
        }

        [Remote]
        public void TurnPassed(int day)
        {
            Signals.PublishTurnPassedEvent(day);
        }

        #endregion
    }
}
