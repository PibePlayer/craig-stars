using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class NewGameMenu : MarginContainer
    {
        Loader loader;
        CheckButton fastHotseatCheckButton;
        Button startButton;
        Button backButton;

        NewGameOptions newGameOptions;
        NewGamePlayers newGamePlayers;


        public override void _Ready()
        {
            base._Ready();
            loader = GetNode<Loader>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BottomHBoxContainer/Loader");
            startButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/StartButton");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BackButton");

            newGameOptions = GetNode<NewGameOptions>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/NewGameOptions");
            newGamePlayers = GetNode<NewGamePlayers>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/NewGamePlayers");

            fastHotseatCheckButton = (CheckButton)FindNode("FastHotseatCheckButton");

            fastHotseatCheckButton.Pressed = Settings.Instance.FastHotseat;
            fastHotseatCheckButton.Connect("toggled", this, nameof(OnFastHotseatToggled));
            backButton.Connect("pressed", this, nameof(OnBackPressed));
            startButton.Connect("pressed", this, nameof(OnStartPressed));

            backButton.Disabled = startButton.Disabled = false;

            newGamePlayers.InitPlayersForSinglePlayerGame();

            EventManager.GameStartedEvent += OnGameStarted;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            EventManager.GameStartedEvent -= OnGameStarted;
        }

        void OnFastHotseatToggled(bool toggled)
        {
            Settings.Instance.FastHotseat = toggled;
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }

        void OnStartPressed()
        {
            GameSettings<Player> settings = newGameOptions.GetGameSettings();
            settings.Players = newGamePlayers.Players;

            // start a new game and change to the client view
            Action startGame = () =>
            {
                ServerManager.Instance.NewGame(settings);

                backButton.Disabled = startButton.Disabled = true;
            };

            if (GamesManager.Instance.GameExists(settings.Name))
            {
                CSConfirmDialog.Show($"A game named {settings.Name} already exists. Are you sure you want to overwrite it?", () =>
                {
                    // delete the existing game
                    GamesManager.Instance.DeleteGame(settings.Name);
                    startGame.Invoke();
                });
            }
            else
            {
                startGame.Invoke();
            }

        }

        /// <summary>
        /// The server will notify us when the game is ready
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarted(PublicGameInfo gameInfo, Player player)
        {
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                PlayersManager.Me = player;
                clientView.GameInfo = gameInfo;
                clientView.LocalPlayers = new List<Player>() { player };
            });
        }

    }
}