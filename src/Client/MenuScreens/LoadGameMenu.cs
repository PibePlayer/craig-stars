using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class LoadGameMenu : MarginContainer
    {
        static ILog log = LogManager.GetLogger(typeof(LoadGameMenu));

        Loader loader;
        Button loadButton;
        Button backButton;
        Button deleteButton;

        ItemList gameItemList;

        CSConfirmDialog confirmationDialog;

        string selectedGame = null;

        public override void _Ready()
        {
            loader = GetNode<Loader>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/HBoxContainer/Loader");
            loadButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/HBoxContainer/LoadButton");
            deleteButton = (Button)FindNode("DeleteButton");
            gameItemList = (ItemList)FindNode("GameItemList");
            confirmationDialog = GetNode<CSConfirmDialog>("ConfirmationDialog");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/HBoxContainer/MenuButtons/BackButton");

            backButton.Connect("pressed", this, nameof(OnBackPressed));

            loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));
            gameItemList.Connect("item_activated", this, nameof(OnGameListItemActivated));

            UpdateItemList();
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
                    Settings.Instance.ShouldContinueGame = true;

                    loader.LoadScene("res://src/Client/GameView.tscn");
                    loadButton.Disabled = backButton.Disabled = true;

                }
            }
        }

        void OnDeleteButtonPressed()
        {
            var selected = gameItemList.GetSelectedItems();
            if (selected.Length == 1)
            {
                var gameFile = gameItemList.GetItemText(selected[0]);
                confirmationDialog.Show($"Are you sure you want to delete the game {gameFile}?",
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