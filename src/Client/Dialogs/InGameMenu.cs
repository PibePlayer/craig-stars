using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{
    public class InGameMenu : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        Button saveTurnButton;
        Button exitGameButton;
        Button exitToMainMenuButton;

        public override void _Ready()
        {
            base._Ready();
            saveTurnButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/SaveTurnButton");
            exitGameButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitGameButton");
            exitToMainMenuButton = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/ExitToMainMenuButton");

            saveTurnButton.Connect("pressed", this, nameof(OnSaveTurnButtonPressed));
            exitGameButton.Connect("pressed", this, nameof(OnExitGameButtonPressed));
            exitToMainMenuButton.Connect("pressed", this, nameof(OnExitToMainMenuButtonPressed));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
            DialogManager.DialogRefCount = 0;

            EventManager.PlayerDirtyChangedEvent += OnPlayerDirtyChanged;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                DialogManager.DialogRefCount = 0;
                EventManager.PlayerDirtyChangedEvent -= OnPlayerDirtyChanged;
            }
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        void OnVisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                DialogManager.DialogRefCount++;
            }
            else
            {
                CallDeferred(nameof(DecrementDialogRefCount));
            }
        }

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }

        void OnPlayerDirtyChanged()
        {
            saveTurnButton.Disabled = !Me.Dirty;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            // if the user presses escape and we don't have other dialogs open, show this one
            var refCount = DialogManager.DialogRefCount;
            if (Input.IsActionJustPressed("in_game_menu") && refCount <= 0)
            {
                DialogManager.DialogRefCount = 0;
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

        async void OnSaveTurnButtonPressed()
        {
            await GamesManager.Instance.SavePlayer(GameInfo, Me);
            Me.Dirty = false;
            EventManager.PublishPlayerDirtyEvent();
        }

        void OnExitGameButtonPressed()
        {
            if (Me.Dirty)
            {
                CSConfirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () =>
                {
                    EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
                    GetTree().Quit();
                });
            }
            else
            {
                EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
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
                    EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
                    ServerManager.Instance.ExitGame();
                    GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
                });
            }
            else
            {
                EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
                ServerManager.Instance.ExitGame();
                GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
            }
        }

    }
}