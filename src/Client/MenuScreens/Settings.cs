using Godot;
using System;

public class Rules : MarginContainer
{
    CheckButton musicCheckButton;

    public override void _Ready()
    {
        musicCheckButton = (CheckButton)FindNode("MusicCheckButton");
        // musicCheckButton.Pressed = GameSettings.Instance.PlayMusic;

        musicCheckButton.Connect("toggled", this, nameof(OnMusicCheckButtonToggled));
        ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));
    }

    void OnMusicCheckButtonToggled(bool buttonPressed)
    {
        // GameSettings.Instance.PlayMusic = buttonPressed;
    }

    void OnBackPressed()
    {
        GetTree().ChangeScene("res://src/Client/Main.tscn");
    }
}
