using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Singletons
{
    public class RPC : Node
    {

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
            GD.Print($"{LogPrefix} Sending Message {playerMessage}");
            Rpc(nameof(Message), playerMessage.ToArray());
        }

        public void SendAllMessages(int networkId)
        {
            GD.Print($"{LogPrefix} Sending All Messages to {networkId}");
            PlayersManager.Instance.Messages.ForEach(m => RpcId(networkId, nameof(Message), m.ToArray()));
        }

        [RemoteSync]
        public void Message(Godot.Collections.Array data)
        {
            var message = new PlayerMessage().FromArray(data);

            GD.Print($"{LogPrefix} Received PlayerMessage {message} from {GetTree().GetRpcSenderId()}");

            // notify listeners that we have updated Player
            Signals.PublishPlayerMessageEvent(message);
        }

        public void SendPlayerUpdated(Player player)
        {
            // send our peers an update of a player
            GD.Print($"{LogPrefix} Notifying clients about player update: {player}");
            Rpc(nameof(PlayerUpdated), player.ToArray());
        }

        [Remote]
        public void PlayerUpdated(Godot.Collections.Array data)
        {
            var player = new Player().FromArray(data);
            player.FromArray(data);

            GD.Print($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

            // notify listeners that we have updated Player
            Signals.PublishPlayerUpdatedEvent(player);

        }

        /// <summary>
        /// Called by the server to notify all clients about new player data
        /// </summary>
        /// <param name="players"></param>
        /// <param name="networkId"></param>
        public void SendPlayersUpdated(List<Player> players, int networkId = 0)
        {
            // servers listen for signals and notify clients
            if (this.IsServer())
            {
                var playersArray = new Godot.Collections.Array(players.Select(p => p.ToArray()));
                if (networkId == 0)
                {
                    // GD.Print($"{LogPrefix} Sending all players to all clients");
                    // we are a server, tell the clients we have a player update
                    Rpc(nameof(PlayersUpdated), playersArray);
                }
                else
                {
                    GD.Print($"{LogPrefix} Sending players to {networkId}");
                    // we are a server, tell the clients we have a player update
                    RpcId(networkId, nameof(PlayersUpdated), playersArray);
                }
            }
            else
            {
                GD.PrintErr("A client tried to send a list of all players over Rpc");
            }
        }

        /// <summary>
        /// Method called by the server whenever a client needs to know about player updates
        /// </summary>
        /// <param name="data"></param>
        [Remote]
        public void PlayersUpdated(Godot.Collections.Array data)
        {
            var players = new Player[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                var Player = data[i] as Godot.Collections.Array;
                if (Player != null)
                {
                    var player = new Player().FromArray(Player);

                    // GD.Print($"{LogPrefix} Received PlayerUpdated event for Player {player.Num} - {player.Name} (NetworkId: {player.NetworkId}");

                    // notify listeners that we have updated Player
                    Signals.PublishPlayerUpdatedEvent(player);
                }
                else
                {
                    GD.PrintErr("Failed to convert array of player arrays in Player: " + data[i].ToString());
                }
            }
        }

        #endregion

        /// <summary>
        /// Sent by the server to notify players the game is started
        /// </summary>
        /// <param name="networkId"></param>
        public void SendPostStartGame(int networkId = 0)
        {
            if (networkId == 0)
            {
                Rpc(nameof(PostStartGame));
            }
            else
            {
                RpcId(networkId, nameof(PostStartGame));
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
