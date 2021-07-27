using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class LoadGameMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LoadGameMenu));

        Loader loader;
        Button loadButton;
        Button backButton;
        Button deleteButton;

        ItemList gameItemList;

        string selectedGame = null;

        public override void _Ready()
        {
            base._Ready();
            loader = GetNode<Loader>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/HBoxContainer/Loader");
            loadButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/HBoxContainer/LoadButton");
            deleteButton = (Button)FindNode("DeleteButton");
            gameItemList = (ItemList)FindNode("GameItemList");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/BackButton");

            backButton.Connect("pressed", this, nameof(OnBackPressed));

            loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));
            gameItemList.Connect("item_activated", this, nameof(OnGameListItemActivated));

            UpdateItemList();

            EventManager.GameStartedEvent += OnGameStarted;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            EventManager.GameStartedEvent -= OnGameStarted;
        }

        void OnGameSaved(Game game, string filename)
        {
            UpdateItemList();
        }

        void UpdateItemList()
        {
            gameItemList.Clear();
            foreach (var game in GamesManager.Instance.GetSavedGames())
            {
                gameItemList.AddItem(game);
            }
        }

        void OnGameListItemActivated(int index)
        {
            OnLoadButtonPressed();
        }

        void OnLoadButtonPressed()
        {
            var selected = gameItemList.GetSelectedItems();
            if (selected.Length == 1)
            {
                var gameFile = gameItemList.GetItemText(selected[0]);
                var gameYears = GamesManager.Instance.GetSavedGameYears(gameFile);
                if (gameYears.Count > 0)
                {
                    var gameYear = gameYears[gameYears.Count - 1];
                    Settings.Instance.ContinueGame = gameFile;
                    Settings.Instance.ContinueYear = gameYear;

                    loadButton.Disabled = backButton.Disabled = true;
                    ServerManager.Instance.ContinueGame(gameFile, gameYear);
                }
            }
        }

        /// <summary>
        /// The server will notify us when the game is ready
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarted(PublicGameInfo gameInfo, Player player)
        {
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (client) =>
            {
                PlayersManager.Me = player;
                client.GameInfo = gameInfo;
            });
        }

        void OnDeleteButtonPressed()
        {
            var selected = gameItemList.GetSelectedItems();
            if (selected.Length == 1)
            {
                var gameFile = gameItemList.GetItemText(selected[0]);
                CSConfirmDialog.Show($"Are you sure you want to delete the game {gameFile}?",
                () =>
                {
                    GamesManager.Instance.DeleteGame(gameFile);
                    UpdateItemList();
                });
            }
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }
    }
}