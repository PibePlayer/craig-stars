using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class InGameMenu : WindowDialog
    {
        Button saveTurnButton;
        Button exitGameButton;
        Button exitToMainMenuButton;
        CSConfirmDialog confirmDialog;

        public override void _Ready()
        {
            saveTurnButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/SaveTurnButton");
            exitGameButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitGameButton");
            exitToMainMenuButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitToMainMenuButton");
            confirmDialog = GetNode<CSConfirmDialog>("ConfirmDialog");

            saveTurnButton.Connect("pressed", this, nameof(OnSaveGameButtonPressed));
            exitGameButton.Connect("pressed", this, nameof(OnExitGameButtonPressed));
            exitToMainMenuButton.Connect("pressed", this, nameof(OnExitToMainMenuButtonPressed));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));

        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionReleased("in_game_menu"))
            {
                PopupCentered();
            }
        }

        void OnAboutToShow()
        {
            GetTree().Paused = true;
        }

        void OnPopupHide()
        {
            GetTree().Paused = false;
        }

        void OnSaveGameButtonPressed()
        {
            // TODO: do this
        }

        void OnExitGameButtonPressed()
        {
            if (PlayersManager.Me.Dirty)
            {
                confirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () => GetTree().Quit());
            }
            else
            {
                GetTree().Quit();
            }
        }

        void OnExitToMainMenuButtonPressed()
        {
            if (PlayersManager.Me.Dirty)
            {
                confirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () => GetTree().ChangeScene("res://src/Client/MainMenu.tscn"));
            }
            else
            {
                GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
            }
        }

    }
}