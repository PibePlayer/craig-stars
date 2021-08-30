using Godot;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    /// <summary>
    /// Special Hoster node for quickly launching a hosted game
    /// </summary>
    public class Joiner : Control
    {
        public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        RPC rpc;

        public override void _Ready()
        {
            rpc = RPC.Instance(GetTree());
            rpc.PlayerJoinedEvent += OnPlayerJoined;
            rpc.PlayerMessageEvent += OnPlayerMessage;

            NetworkClient.Instance.JoinGame(Settings.Instance.ClientHost, Settings.Instance.ClientPort);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                rpc.PlayerJoinedEvent -= OnPlayerJoined;
                rpc.PlayerMessageEvent -= OnPlayerMessage;
            }
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        void OnPlayerJoined(PublicPlayerInfo player)
        {
            // once our player is updated from the server, go to the lobby
            if (this.IsClient() && player.NetworkId == this.GetNetworkId())
            {
                GoToLobby();
            }
        }

        void GoToLobby()
        {
            this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
            {
                instance.InitialMessages.AddRange(Messages);
            });
        }
    }
}
