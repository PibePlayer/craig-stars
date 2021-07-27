using CraigStars;
using Godot;
using System;

namespace CraigStars.Client
{
    public class SettingsMenu : MarginContainer
    {
        CheckButton fastHotseatCheckButton;

        public override void _Ready()
        {
            fastHotseatCheckButton = (CheckButton)FindNode("FastHotseatCheckButton");

            fastHotseatCheckButton.Pressed = Settings.Instance.FastHotseat;
            fastHotseatCheckButton.Connect("toggled", this, nameof(OnFastHotseatToggled));

            ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));
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