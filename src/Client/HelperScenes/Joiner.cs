using Godot;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// Special Hoster node for quickly launching a hosted game
    /// </summary>
    public class Joiner : Control
    {
        public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        public override void _Ready()
        {
            RPC.Instance(GetTree()).PlayerJoinedEvent += OnPlayerJoined;
            RPC.Instance(GetTree()).PlayerMessageEvent += OnPlayerMessage;

            NetworkClient.Instance.JoinGame(Settings.Instance.ClientHost, Settings.Instance.ClientPort);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.PlayerUpdatedEvent -= OnPlayerJoined;
            RPC.Instance(GetTree()).PlayerMessageEvent -= OnPlayerMessage;
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
            this.ChangeSceneTo<Lobby>("res://src/Client/MenuScreens/Lobby.tscn", (instance) =>
            {
                instance.InitialMessages.AddRange(Messages);
            });
        }
    }
}
