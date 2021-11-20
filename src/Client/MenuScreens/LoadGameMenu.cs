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

        PlayerSavesTable savesTable;

        PlayerSave selectedSave;

        List<PlayerSave> saves;

        public override async void _Ready()
        {
            base._Ready();
            loadButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/HBoxContainer/LoadButton");
            loadButton.GrabFocus();
            deleteButton = (Button)FindNode("DeleteButton");
            savesTable = (PlayerSavesTable)FindNode("SavesTable");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BackButton");

            backButton.Connect("pressed", this, nameof(OnBackPressed));

            loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

            savesTable.Data.AddColumn("Name");
            savesTable.Data.AddColumn("Mode");
            savesTable.Data.AddColumn("Size");
            savesTable.Data.AddColumn("Year");
            savesTable.Data.AddColumn("Player");
            savesTable.Data.AddColumn("Players", align: Label.AlignEnum.Right);

            // load the full games and update the UI
            await Task.Run(() => saves = GamesManager.Instance.GetPlayerSaves());
            savesTable.RowSelectedEvent += OnRowSelected;
            savesTable.RowActivatedEvent += OnRowActivated;
            UpdateItemList();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                savesTable.RowSelectedEvent -= OnRowSelected;
            }
        }

        void OnRowSelected(int rowIndex, int colIndex, Cell cell, PlayerSave metadata)
        {
            selectedSave = metadata;
        }

        void OnRowActivated(int rowIndex, int colIndex, Cell cell, PlayerSave metadata)
        {
            selectedSave = metadata;
            OnLoadButtonPressed();
        }


        void OnGameSaved(Game game, string filename)
        {
            UpdateItemList();
        }

        void UpdateItemList()
        {
            savesTable.Data.ClearRows();
            selectedSave = null;
            if (saves != null)
            {
                foreach (var save in saves)
                {
                    if (save.GameInfo.Mode == GameMode.Hotseat && save.PlayerNum != 0)
                    {
                        // only show the first hotseat player
                        continue;
                    }
                    savesTable.Data.AddRowAdvanced(save, Colors.White, false,
                        save.GameInfo.Name,
                        save.GameInfo.Mode.ToString(),
                        save.GameInfo.Size.ToString(),
                        save.GameInfo.Year,
                        save.GameInfo.Players[save.PlayerNum].Name,
                        save.GameInfo.Players.Count
                    );
                }
            }

            savesTable.ResetTable();
        }

        void OnGameListItemActivated(int index)
        {
            OnLoadButtonPressed();
        }

        void OnLoadButtonPressed()
        {
            if (selectedSave != null)
            {
                Settings.Instance.ContinueGame = selectedSave.GameInfo.Name;
                Settings.Instance.ContinueYear = selectedSave.GameInfo.Year;

                loadButton.Disabled = backButton.Disabled = true;

                // for multiplayer games, load only our player save.
                // For singleplayer/hotseat games, load all players
                var (gameInfo, players) = ServerManager.Instance.ContinueGame(
                    selectedSave.GameInfo.Name,
                    selectedSave.GameInfo.Year,
                    selectedSave.GameInfo.Mode == GameMode.NetworkedMultiPlayer ? selectedSave.PlayerNum : -1
                );
                this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
                {
                    clientView.GameInfo = gameInfo;
                    clientView.LocalPlayers = players;
                });
            }
        }

        void OnDeleteButtonPressed()
        {
            if (selectedSave != null)
            {
                GamesManager.Instance.DeleteGame(selectedSave.GameInfo.Name);
                CSConfirmDialog.Show($"Are you sure you want to delete the game {selectedSave.GameInfo.Name}?",
                () =>
                {
                    GamesManager.Instance.DeleteGame(selectedSave.GameInfo.Name);
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