using CraigStars;
using Godot;
using System;

namespace CraigStars.Client
{
    public class SettingsMenu : MarginContainer
    {
        CheckButton fastHotseatCheckButton;
        LineEdit nameLineEdit;

        public override void _Ready()
        {
            nameLineEdit = (LineEdit)FindNode("NameLineEdit");
            nameLineEdit.GrabFocus();
            fastHotseatCheckButton = (CheckButton)FindNode("FastHotseatCheckButton");

            fastHotseatCheckButton.Pressed = Settings.Instance.FastHotseat;
            fastHotseatCheckButton.Connect("toggled", this, nameof(OnFastHotseatToggled));

            nameLineEdit.Text = Settings.Instance.PlayerName;
            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));

            ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));
        }

        void OnNameLineEditTextChanged(string newText)
        {
            Settings.Instance.PlayerName = newText;
        }

        void OnFastHotseatToggled(bool toggled)
        {
            Settings.Instance.FastHotseat = toggled;
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }
    }
}