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

            [Option("continue-year", Required = false, HelpText = "The year to continue", Default = -1)]
            public int ContinueYear { get; set; }

            [Option("continue-game", Required = false, HelpText = "The game to continue")]
            public string ContinueGame { get; set; }

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
            // start loading resources from disk
            CSTableResourceLoader.Instance.StartPreLoad();
            CSResourceLoader.Instance.StartPreload();

            // switch to the scene we want
            if (options.QuickStart)
            {
                this.ChangeSceneTo<NewGameMenu>("res://src/Client/MenuScreens/NewGameMenu.tscn", (node) =>
                {
                    node.QuickStart = true;
                });
            }
            else
            {
                this.ChangeSceneTo<MainMenu>("res://src/Client/MainMenu.tscn", (node) =>
                {
                    node.Continue = options.Continue;
                    node.ContinueGame = options.ContinueGame;
                    node.ContinueYear = options.ContinueYear;
                });
            }
        }

        void Exit()
        {
            GD.Print("Hello World");
            GetTree().Quit();
        }
    }
}