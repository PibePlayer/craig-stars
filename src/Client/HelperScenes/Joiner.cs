using Godot;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Linq;
using CommandLine;

namespace CraigStars.Client
{
    /// <summary>
    /// Special Hoster node for quickly launching a hosted game
    /// </summary>
    public class Joiner : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(Joiner));

        public List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();

        RPC rpc;

        public class Options
        {
            [Option('n', "player-name", Required = true, HelpText = "The player to join a game as.")]
            public string PlayerName { get; set; }
        }

        string playerName;

        public override void _Ready()
        {
            rpc = RPC.Instance(GetTree());
            rpc.PlayerJoinedNewGameEvent += OnPlayerJoinedNewGame;
            rpc.PlayerJoinedExistingGameEvent += OnPlayerJoinedExistingGame;
            rpc.PlayerMessageEvent += OnPlayerMessage;

            Parser.Default.ParseArguments<Options>(OS.GetCmdlineArgs()).WithParsed<Options>(o =>
            {
                playerName = o.PlayerName;
                log.Info($"Setting PlayerName to {playerName}");
            });

            NetworkClient.Instance.JoinGame(Settings.Instance.ClientHost, Settings.Instance.ClientPort);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                rpc.PlayerJoinedNewGameEvent -= OnPlayerJoinedNewGame;
                rpc.PlayerJoinedExistingGameEvent -= OnPlayerJoinedExistingGame;
                rpc.PlayerMessageEvent -= OnPlayerMessage;
            }
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        void OnPlayerJoinedNewGame(PublicPlayerInfo player)
        {
            // once our player is updated from the server, go to the lobby
            if (this.IsClient() && player.NetworkId == this.GetNetworkId())
            {
                this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
                {
                    instance.InitialMessages.AddRange(Messages);
                    instance.PlayerName = playerName;
                });
            }
        }

        private void OnPlayerJoinedExistingGame(PublicGameInfo gameInfo)
        {
            // once our player is updated from the server, go to the lobby
            if (this.IsClient())
            {
                this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
                {
                    clientView.GameInfo = gameInfo;
                });
            }
        }


    }
}
