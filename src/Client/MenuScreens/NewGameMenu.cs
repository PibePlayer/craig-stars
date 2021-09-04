using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class NewGameMenu : MarginContainer
    {
        CheckButton fastHotseatCheckButton;
        Button startButton;
        Button backButton;

        NewGameOptions newGameOptions;
        NewGamePlayers newGamePlayers;


        public override void _Ready()
        {
            base._Ready();
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

            EventManager.GameStartingEvent += OnGameStarting;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameStartingEvent -= OnGameStarting;
            }
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
            settings.Mode = settings.Players.Where(p => !p.AIControlled).Count() > 1 ? GameMode.Hotseat : GameMode.SinglePlayer;

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
        void OnGameStarting(PublicGameInfo gameInfo)
        {
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                clientView.GameInfo = gameInfo;
            });
        }

    }
}