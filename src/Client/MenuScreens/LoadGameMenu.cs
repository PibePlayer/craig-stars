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
                // TODO: this doesn't work if you aren't player 0...
                var gameYears = GamesManager.Instance.GetSavedGameYears(selectedGame.Name, playerSaves: true, playerNum: 0);
                if (gameYears.Count > 0)
                {
                    var gameYear = gameYears[gameYears.Count - 1];
                    Settings.Instance.ContinueGame = selectedGame.Name;
                    Settings.Instance.ContinueYear = gameYear;

                    loadButton.Disabled = backButton.Disabled = true;

                    var (gameInfo, players) = ServerManager.Instance.ContinueGame(selectedGame.Name, gameYear);
                    this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
                    {
                        clientView.GameInfo = gameInfo;
                        clientView.LocalPlayers = players;
                    });

                }
            }
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