using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class PlayerChooser : VBoxContainer
    {
        /// <summary>
        /// If the player is removed, this event is triggered
        /// </summary>
        public Action<PlayerChooser, Player> PlayerRemovedEvent;

        public Player Player
        {
            get => player;
            set
            {
                player = value;
                UpdateControls();
            }
        }
        Player player = new Player();

        Label playerNumLabel;
        LineEdit nameLineEdit;

        Control raceOptionsHBoxContainer;
        OptionButton raceOptionButton;
        Button newRaceButton;
        Button editRaceButton;

        Control aiHBoxContainer;
        CheckBox aiControlledCheckBoxButton;
        OptionButton aiDifficultyOptionButton;

        ColorPickerButton colorPickerButton;

        Button removePlayerButton;

        RaceDesignerDialog raceDesignerDialog;

        public override void _Ready()
        {
            playerNumLabel = GetNode<Label>("HBoxContainer/PlayerNumLabel");
            nameLineEdit = GetNode<LineEdit>("HBoxContainer/VBoxContainer/NameHBoxContainer/NameLineEdit");

            aiControlledCheckBoxButton = GetNode<CheckBox>("HBoxContainer/VBoxContainer/NameHBoxContainer/AIControlledCheckBox");
            aiDifficultyOptionButton = GetNode<OptionButton>("HBoxContainer/VBoxContainer/AIHBoxContainer/AIDifficultyOptionButton");
            aiHBoxContainer = GetNode<Container>("HBoxContainer/VBoxContainer/AIHBoxContainer");

            raceOptionsHBoxContainer = GetNode<Control>("HBoxContainer/VBoxContainer/RaceOptionsHBoxContainer");
            raceOptionButton = GetNode<OptionButton>("HBoxContainer/VBoxContainer/RaceOptionsHBoxContainer/RaceOptionButton");
            newRaceButton = GetNode<Button>("HBoxContainer/VBoxContainer/RaceOptionsHBoxContainer/NewRaceButton");
            editRaceButton = GetNode<Button>("HBoxContainer/VBoxContainer/RaceOptionsHBoxContainer/EditRaceButton");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("RaceDesignerDialog");

            colorPickerButton = GetNode<ColorPickerButton>("HBoxContainer/VBoxContainer/HBoxContainer/ColorPickerButton");

            removePlayerButton = GetNode<Button>("HBoxContainer/RemovePlayerButton");

            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));

            newRaceButton.Connect("pressed", this, nameof(OnNewRaceButtonPressed));
            editRaceButton.Connect("pressed", this, nameof(OnEditRaceButtonPressed));
            raceOptionButton.Connect("item_selected", this, nameof(OnRaceOptionButtonItemSelected));
            aiControlledCheckBoxButton.Connect("pressed", this, nameof(UpdateAIControls));
            aiDifficultyOptionButton.PopulateOptionButton<AIDifficulty>();
            colorPickerButton.Connect("color_changed", this, nameof(OnColorChanged));
            removePlayerButton.Connect("pressed", this, nameof(OnRemovePlayerButtonPressed));

            UpdateRaceFiles();
            UpdateControls();

            EventManager.RaceSavedEvent += OnRaceSaved;

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.RaceSavedEvent -= OnRaceSaved;
            }
        }

        public bool Validate()
        {
            raceOptionButton.Modulate = Colors.White;
            nameLineEdit.Modulate = Colors.White;

            var valid = true;
            if (nameLineEdit.Text.Trim() == "")
            {
                nameLineEdit.Modulate = Colors.Red;
                valid = false;
            }
            if (!Player.AIControlled && raceOptionButton.Selected == -1)
            {
                raceOptionButton.Modulate = Colors.Red;
                valid = false;
            }
            return valid;
        }

        void OnNameLineEditTextChanged(string newText)
        {
            Player.Name = newText;
        }

        void UpdateAIControls()
        {
            if (Player != null)
            {
                Player.AIControlled = aiControlledCheckBoxButton.Pressed;
                if (Player.AIControlled)
                {
                    raceOptionsHBoxContainer.Visible = false;
                    aiHBoxContainer.Visible = true;
                    aiDifficultyOptionButton.Select((int)Player.AIDifficulty);
                }
                else
                {
                    Player.Race = Races.Humanoid;
                    raceOptionsHBoxContainer.Visible = true;
                    aiHBoxContainer.Visible = false;
                }
            }
        }

        void OnRaceSaved(Race race, string filename)
        {
            UpdateRaceFiles(filename);
        }

        void OnRaceOptionButtonItemSelected(int index)
        {
            Player.Race = RacesManager.LoadRace(raceOptionButton.GetItemText(raceOptionButton.Selected));
        }

        void OnNewRaceButtonPressed()
        {
            raceDesignerDialog.Race = new Race();
            raceDesignerDialog.PopupCentered();
        }

        void OnEditRaceButtonPressed()
        {
            var selectedRace = RacesManager.LoadRace(raceOptionButton.GetItemText(raceOptionButton.Selected));
            raceDesignerDialog.Race = selectedRace;
            raceDesignerDialog.PopupCentered();
        }

        void OnColorChanged(Color color)
        {
            Player.Color = color;
        }

        void OnRemovePlayerButtonPressed()
        {
            PlayerRemovedEvent?.Invoke(this, Player);
        }

        void UpdateRaceFiles(string selected = null)
        {
            raceOptionButton.Clear();
            RacesManager.GetRaceFiles().Each((raceFile, index) =>
            {
                raceOptionButton.AddItem(raceFile);
                if (raceFile == selected)
                {
                    raceOptionButton.Select(index);
                }
            });

            if (selected == null)
            {
                raceOptionButton.Selected = -1;
                raceOptionButton.Text = "Choose race...";
            }
        }

        internal void UpdateControls()
        {
            if (Player != null && playerNumLabel != null)
            {
                playerNumLabel.Text = $"{player.Num + 1}";
                colorPickerButton.Color = player.Color;
                aiControlledCheckBoxButton.Pressed = player.AIControlled;
                nameLineEdit.Text = player.Name;

                UpdateAIControls();
            }
        }
    }
}