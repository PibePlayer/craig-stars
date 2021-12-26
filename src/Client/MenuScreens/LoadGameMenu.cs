using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

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
                    if (save.GameInfo.Mode == GameMode.HotseatMultiplayer && save.PlayerNum != 0)
                    {
                        // only show the first hotseat player
                        continue;
                    }
                    savesTable.Data.AddRowAdvanced(save, Colors.White, false,
                        save.GameInfo.Name,
                        save.GameInfo.Mode.ToString(),
                        EnumUtils.GetLabelForSize(save.GameInfo.Size),
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
                loadButton.Disabled = backButton.Disabled = true;

                var gameInfo = selectedSave.GameInfo;
                var playerNum = selectedSave.PlayerNum;

                try
                {
                    var continuer = GetNode<Continuer>("Continuer");

                    continuer.Continue(gameInfo.Name, gameInfo.Year, playerNum);

                    Settings.Instance.ContinueGame = gameInfo.Name;
                    Settings.Instance.ContinueYear = gameInfo.Year;
                    Settings.Instance.ContinuePlayerNum = playerNum;
                }
                catch (Exception e)
                {
                    log.Error($"Failed to continue game {gameInfo.Name}: {gameInfo.Year}", e);
                    CSConfirmDialog.Show($"Failed to load game {gameInfo.Name}: {gameInfo.Year}");
                }
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