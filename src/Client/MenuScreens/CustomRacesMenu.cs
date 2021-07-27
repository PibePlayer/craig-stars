using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class CustomRacesMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(CustomRacesMenu));

        Button newButton;
        Button editButton;
        Button deleteButton;

        ItemList raceItemList;

        RaceDesignerDialog raceDesignerDialog;

        public override void _Ready()
        {
            newButton = (Button)FindNode("NewButton");
            editButton = (Button)FindNode("EditButton");
            deleteButton = (Button)FindNode("DeleteButton");
            raceItemList = (ItemList)FindNode("RaceItemList");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("RaceDesignerDialog");

            ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));

            newButton.Connect("pressed", this, nameof(OnNewButtonPressed));
            editButton.Connect("pressed", this, nameof(OnEditButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

            EventManager.RaceSavedEvent += OnRaceSaved;

            UpdateItemList();
        }

        public override void _ExitTree()
        {
            EventManager.RaceSavedEvent -= OnRaceSaved;
        }

        void OnRaceSaved(Race race, string filename)
        {
            UpdateItemList();
        }

        void UpdateItemList()
        {
            raceItemList.Clear();
            foreach (var raceFile in RacesManager.GetRaceFiles())
            {
                raceItemList.AddItem(raceFile);
            }
        }

        void OnNewButtonPressed()
        {
            raceDesignerDialog.Race = Races.Humanoid;
            raceDesignerDialog.PopupCentered();
        }

        void OnEditButtonPressed()
        {
            var selected = raceItemList.GetSelectedItems();
            if (selected.Length == 1)
            {
                var raceFile = raceItemList.GetItemText(selected[0]);
                var race = RacesManager.LoadRace(raceFile);
                if (race != null)
                {
                    raceDesignerDialog.Race = race;
                    raceDesignerDialog.Filename = raceFile;
                    raceDesignerDialog.PopupCentered();
                }
                else
                {
                    log.Error($"Failed to load race from file {raceFile}");
                }
            }
        }

        void OnDeleteButtonPressed()
        {
            var selected = raceItemList.GetSelectedItems();
            if (selected.Length == 1)
            {
                var raceFile = raceItemList.GetItemText(selected[0]);
                CSConfirmDialog.Show($"Are you sure you want to delete the race {raceFile}?",
                () =>
                {
                    RacesManager.DeleteRace(raceFile);
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