using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class InGameMenu : GameViewDialog
    {
        Loader loader;
        Button saveTurnButton;
        Button exitGameButton;
        Button exitToMainMenuButton;

        public override void _Ready()
        {
            base._Ready();
            loader = GetNode<Loader>("MarginContainer/CenterContainer/VBoxContainer/Loader");
            saveTurnButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/SaveTurnButton");
            exitGameButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitGameButton");
            exitToMainMenuButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitToMainMenuButton");

            saveTurnButton.Connect("pressed", this, nameof(OnSaveTurnButtonPressed));
            exitGameButton.Connect("pressed", this, nameof(OnExitGameButtonPressed));
            exitToMainMenuButton.Connect("pressed", this, nameof(OnExitToMainMenuButtonPressed));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            DialogManager.DialogRefCount = 0;

            Signals.PlayerDirtyChangedEvent += OnPlayerDirtyChanged;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            DialogManager.DialogRefCount = 0;
            Signals.PlayerDirtyChangedEvent -= OnPlayerDirtyChanged;
        }

        void OnPlayerDirtyChanged()
        {
            saveTurnButton.Disabled = !Me.Dirty;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            // if the user presses escape and we don't have other dialogs open, show this one
            if (Input.IsActionJustPressed("in_game_menu") && DialogManager.DialogRefCount == 0)
            {
                PopupCentered();
            }

            if (Input.IsActionPressed("save_turn"))
            {
                OnSaveTurnButtonPressed();
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

        void OnSaveTurnButtonPressed()
        {
            GamesManager.Instance.SavePlayer(Me);
            Me.Dirty = false;
            Signals.PublishPlayerDirtyEvent();
        }

        void OnExitGameButtonPressed()
        {
            if (Me.Dirty)
            {
                CSConfirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () => GetTree().Quit());
            }
            else
            {
                GetTree().Quit();
            }
        }

        void OnExitToMainMenuButtonPressed()
        {
            if (Me.Dirty)
            {
                CSConfirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () =>
                {
                    ServerManager.Instance.ExitGame();
                    loader.LoadScene("res://src/Client/MainMenu.tscn");
                });
            }
            else
            {
                ServerManager.Instance.ExitGame();
                loader.LoadScene("res://src/Client/MainMenu.tscn");
            }
        }

    }
}