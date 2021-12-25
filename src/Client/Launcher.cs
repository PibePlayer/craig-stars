using System;
using CommandLine;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    public class Launcher : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(Launcher));

        public class Options
        {
            [Option('n', "player-name", Required = false, HelpText = "The player to join a game as.")]
            public string PlayerName { get; set; }

            [Option("continue", Required = false, HelpText = "Continue an existing game")]
            public bool Continue { get; set; }

            [Option("year", Required = false, HelpText = "The year to continue", Default = -1)]
            public int Year { get; set; }

            [Option("game", Required = false, HelpText = "The game to continue")]
            public string GameName { get; set; }

            [Option("start-server", Required = false, HelpText = "Start a dedicated server.")]
            public bool StartServer { get; set; }

            [Option("join-server", Required = false, HelpText = "Join a multiplayer server by ip or address.")]
            public string JoinServer { get; set; }

            [Option("port", Required = false, HelpText = "The port to host or connect to", Default = 3000)]
            public int Port { get; set; }

            [Option("quick-start", Required = false, HelpText = "Start a quick new game (warning, erases the default new game.")]
            public bool QuickStart { get; set; }

        }

        Options options;

        public override void _Ready()
        {
            Parser.Default.ParseArguments<Options>(OS.GetCmdlineArgs())
                .WithParsed<Options>(o =>
                {
                    options = o;
                    o.PlayerName = o.PlayerName ?? Settings.Instance.PlayerName;
                    log.Info($"Setting PlayerName to {o.PlayerName}");

                    CallDeferred(nameof(ChangeScene));
                })
                .WithNotParsed<Options>(o =>
                {
                    CallDeferred(nameof(Exit));
                });
        }

        void ChangeScene()
        {
            // switch to the scene we want
            if (options.QuickStart)
            {
                // start loading resources from disk
                CSTableResourceLoader.Instance.StartPreLoad();
                CSResourceLoader.Instance.StartPreload();

                this.ChangeSceneTo<NewGameMenu>("res://src/Client/MenuScreens/NewGameMenu.tscn", (node) =>
                {
                    node.QuickStart = true;
                });
            }
            else if (options.StartServer)
            {
                this.ChangeSceneTo<DedicatedServer>("res://src/Server/DedicatedServer.tscn", (node) =>
                {
                    node.GameName = options.GameName;
                    node.Year = options.Year;
                    node.Port = options.Port;
                });
            }
            else
            {
                // start loading resources from disk
                CSTableResourceLoader.Instance.StartPreLoad();
                CSResourceLoader.Instance.StartPreload();
                this.ChangeSceneTo<MainMenu>("res://src/Client/MainMenu.tscn", (node) =>
                {
                    node.Continue = options.Continue;
                    node.GameName = options.GameName;
                    node.Year = options.Year;
                    node.JoinServer = options.JoinServer;
                    node.Port = options.Port;
                });
            }
        }

        /// <summary>
        /// Called when we fail to parse command line args
        /// </summary>
        void Exit()
        {
            GetTree().Quit();
        }
    }
}