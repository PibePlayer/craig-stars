using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars.Client
{

    public class NewGamePlayer : PlayerChooser<Player>
    {
        LineEdit nameLineEdit;
        ColorPickerButton colorPickerButton;

        Control raceOptionsHBoxContainer;
        OptionButton raceOptionButton;
        Button newRaceButton;
        Button editRaceButton;
        RaceDesignerDialog raceDesignerDialog;

        Control aiHBoxContainer;
        CheckBox aiControlledCheckBoxButton;
        OptionButton aiDifficultyOptionButton;

        RPC rpc;

        public override void _Ready()
        {
            rpc = RPC.Instance(GetTree());

            nameLineEdit = GetNode<LineEdit>("HBoxContainer/PlayerDetails/NameHBoxContainer/NameLineEdit");

            aiControlledCheckBoxButton = GetNode<CheckBox>("HBoxContainer/PlayerDetails/NameHBoxContainer/AIControlledCheckBox");
            aiDifficultyOptionButton = GetNode<OptionButton>("HBoxContainer/PlayerDetails/AIHBoxContainer/AIDifficultyOptionButton");
            aiHBoxContainer = GetNode<Container>("HBoxContainer/PlayerDetails/AIHBoxContainer");

            raceOptionsHBoxContainer = GetNode<Control>("HBoxContainer/PlayerDetails/RaceOptionsHBoxContainer");
            raceOptionButton = GetNode<OptionButton>("HBoxContainer/PlayerDetails/RaceOptionsHBoxContainer/RaceOptionButton");
            newRaceButton = GetNode<Button>("HBoxContainer/PlayerDetails/RaceOptionsHBoxContainer/NewRaceButton");
            editRaceButton = GetNode<Button>("HBoxContainer/PlayerDetails/RaceOptionsHBoxContainer/EditRaceButton");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("RaceDesignerDialog");

            colorPickerButton = GetNode<ColorPickerButton>("HBoxContainer/PlayerDetails/HBoxContainer/ColorPickerButton");

            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));
            colorPickerButton.Connect("color_changed", this, nameof(OnColorChanged));

            newRaceButton.Connect("pressed", this, nameof(OnNewRaceButtonPressed));
            editRaceButton.Connect("pressed", this, nameof(OnEditRaceButtonPressed));
            raceOptionButton.Connect("item_selected", this, nameof(OnRaceOptionButtonItemSelected));
            aiControlledCheckBoxButton.Connect("pressed", this, nameof(OnAIToggled));
            aiDifficultyOptionButton.PopulateOptionButton<AIDifficulty>();
            aiDifficultyOptionButton.Connect("item_selected", this, nameof(OnAIDifficultyOptionButtonItemSelected));

            EventManager.RaceSavedEvent += OnRaceSaved;

            UpdateRaceFiles();

            if (this.IsMultiplayer())
            {
                NetworkClient.Instance.UpdateRaceOnServer(Player.Race);
                // temporarily disable ai players for multiplayer games
                // until we add the Add Player button for the host
                aiControlledCheckBoxButton.Visible = false;
            }
            else
            {
                aiControlledCheckBoxButton.Visible = true;
            }

            base._Ready();
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
            if (this.IsMultiplayer())
            {
                NetworkClient.Instance.PublishPlayerUpdatedEvent(Player, notifyPeers: true, GetTree());
            }
        }

        void OnColorChanged(Color color)
        {
            Player.Color = color;
            if (this.IsMultiplayer())
            {
                NetworkClient.Instance.PublishPlayerUpdatedEvent(Player, notifyPeers: true, GetTree());
            }
        }

        #region AI Events

        void OnAIDifficultyOptionButtonItemSelected(int index)
        {
            if (Player != null)
            {
                Player.AIDifficulty = (AIDifficulty)index;
            }
        }

        void OnAIToggled()
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

        #endregion

        #region Race Choosing

        void OnRaceSaved(Race race, string filename)
        {
            UpdateRaceFiles(filename);
            if (this.IsMultiplayer())
            {
                NetworkClient.Instance.UpdateRaceOnServer(Player.Race);
            }
        }

        void OnRaceOptionButtonItemSelected(int index)
        {
            Player.Race = RacesManager.LoadRace(raceOptionButton.GetItemText(raceOptionButton.Selected));
            if (this.IsMultiplayer())
            {
                NetworkClient.Instance.UpdateRaceOnServer(Player.Race);
            }
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
                raceOptionButton.Text = "Humanoids (Built In)";
            }
        }

        #endregion

        public override void UpdateControls()
        {
            base.UpdateControls();
            if (Player != null && colorPickerButton != null)
            {
                colorPickerButton.Color = Player.Color;
                aiControlledCheckBoxButton.Pressed = Player.AIControlled;
                nameLineEdit.Text = Player.Name;

                OnAIToggled();
            }
        }

    }
}