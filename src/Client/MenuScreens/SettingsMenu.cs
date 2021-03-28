using Godot;
using System;

public class SettingsMenu : MarginContainer
{
    public override void _Ready()
    {
        ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));
    }

    void OnBackPressed()
    {
        GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
    }
}
