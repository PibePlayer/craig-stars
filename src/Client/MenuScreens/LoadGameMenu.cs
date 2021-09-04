using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CraigStarsTable;

namespace CraigStars.Client
{
    public class LoadGameMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LoadGameMenu));

        Button loadButton;
        Button backButton;
        Button deleteButton;

        PublicGameInfosTable gamesTable;

        PublicGameInfo selectedGame;

        List<PublicGameInfo> gameInfos;

        public override async void _Ready()
        {
            base._Ready();
            loadButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/HBoxContainer/LoadButton");
            deleteButton = (Button)FindNode("DeleteButton");
            gamesTable = (PublicGameInfosTable)FindNode("GamesTable");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BackButton");

            backButton.Connect("pressed", this, nameof(OnBackPressed));

            loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

            gamesTable.Data.AddColumn("Name");
            gamesTable.Data.AddColumn("Mode");
            gamesTable.Data.AddColumn("Size");
            gamesTable.Data.AddColumn("Year");
            gamesTable.Data.AddColumn("Players", align: Label.AlignEnum.Right);

            EventManager.GameStartingEvent += OnGameStarting;

            // load the full games and update the UI
            await Task.Run(() => gameInfos = GamesManager.Instance.GetSavedGameInfos());
            gamesTable.RowSelectedEvent += OnRowSelected;
            gamesTable.RowActivatedEvent += OnRowActivated;
            UpdateItemList();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameStartingEvent -= OnGameStarting;
                gamesTable.RowSelectedEvent -= OnRowSelected;
            }
        }

        void OnRowSelected(int rowIndex, int colIndex, Cell cell, PublicGameInfo metadata)
        {
            selectedGame = metadata;
        }

        void OnRowActivated(int rowIndex, int colIndex, Cell cell, PublicGameInfo metadata)
        {
            selectedGame = metadata;
            OnLoadButtonPressed();
        }


        void OnGameSaved(Game game, string filename)
        {
            UpdateItemList();
        }

        void UpdateItemList()
        {
            gamesTable.Data.ClearRows();
            selectedGame = null;
            if (gameInfos != null)
            {
                foreach (var game in gameInfos)
                {
                    gamesTable.Data.AddRowAdvanced(game, Colors.White, false, game.Name, game.Mode.ToString(), game.Size.ToString(), game.Year, game.Players.Count);
                }
            }

            gamesTable.ResetTable();
        }

        void OnGameListItemActivated(int index)
        {
            OnLoadButtonPressed();
        }

        void OnLoadButtonPressed()
        {
            if (selectedGame != null)
            {
                var gameYears = GamesManager.Instance.GetSavedGameYears(selectedGame.Name);
                if (gameYears.Count > 0)
                {
                    var gameYear = gameYears[gameYears.Count - 1];
                    Settings.Instance.ContinueGame = selectedGame.Name;
                    Settings.Instance.ContinueYear = gameYear;

                    loadButton.Disabled = backButton.Disabled = true;
                    ServerManager.Instance.ContinueGame(selectedGame.Name, gameYear);
                }
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

        void OnDeleteButtonPressed()
        {
            if (selectedGame != null)
            {
                GamesManager.Instance.DeleteGame(selectedGame.Name);
                CSConfirmDialog.Show($"Are you sure you want to delete the game {selectedGame.Name}?",
                () =>
                {
                    GamesManager.Instance.DeleteGame(selectedGame.Name);
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