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

        ServerRPC serverRPC;
        ClientRPC clientRPC;

        public class Options
        {
            [Option('n', "player-name", Required = false, HelpText = "The player to join a game as.")]
            public string PlayerName { get; set; }

            [Option("continue", Required = false, HelpText = "Continue an existing game")]
            public bool ContinueExistingGame { get; set; }

        }

        string playerName;
        bool continueExistingGame;
        Player continuePlayer;

        public override void _Ready()
        {
            serverRPC = ServerRPC.Instance(GetTree());
            clientRPC = ClientRPC.Instance(GetTree());
            clientRPC.PlayerMessageEvent += OnPlayerMessage;

            Parser.Default.ParseArguments<Options>(OS.GetCmdlineArgs()).WithParsed<Options>(o =>
            {
                playerName = o.PlayerName ?? "Bob";
                continueExistingGame = o.ContinueExistingGame;
                log.Info($"Setting PlayerName to {playerName} (continue: {continueExistingGame}");
            });

            CSResourceLoader.SceneLoadCompeteEvent += OnSceneLoadComplete;

            if (!continueExistingGame)
            {
                clientRPC.PlayerJoinedNewGameEvent += OnPlayerJoinedNewGame;
                NetworkClient.Instance.JoinNewGame(Settings.Instance.ClientHost, Settings.Instance.ClientPort);
            }

        }

        void OnSceneLoadComplete()
        {
            if (continueExistingGame)
            {
                CallDeferred(nameof(ContinueGame));
            }
        }

        void ContinueGame()
        {
            var gameInfo = GamesManager.Instance.LoadPlayerGameInfo(Settings.Instance.ContinueGame, Settings.Instance.ContinueYear);
            log.Info($"Continuing game {gameInfo}, with players: {string.Join(", ", gameInfo.Players.Select(p => p.ToString()).ToList())}");
            var playerByName = GamesManager.Instance.GetPlayerSaves(gameInfo).Find(p => p.Name == playerName);
            log.Info($"Continuing with player {playerByName}");
            continuePlayer = GamesManager.Instance.LoadPlayerSave(gameInfo, playerByName.Num);

            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                clientView.GameInfo = gameInfo;
                clientView.LocalPlayers = new List<Player>() { continuePlayer };
            });
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                clientRPC.PlayerJoinedNewGameEvent -= OnPlayerJoinedNewGame;
                clientRPC.PlayerMessageEvent -= OnPlayerMessage;
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


    }
}
