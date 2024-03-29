using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class InGameMenu : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        TurnGenerationStatus turnGenerationStatus;
        Button saveTurnButton;
        Button generateTurnButton;
        Button exitGameButton;
        Button exitToMainMenuButton;

        public override void _Ready()
        {
            base._Ready();

            turnGenerationStatus = GetNode<TurnGenerationStatus>("MarginContainer/VBoxContainer/TurnGenerationStatus");
            saveTurnButton = GetNode<Button>("MarginContainer/VBoxContainer/CenterContainer/VBoxContainer/SaveTurnButton");
            generateTurnButton = GetNode<Button>("MarginContainer/VBoxContainer/CenterContainer/VBoxContainer/GenerateTurnButton");
            exitGameButton = GetNode<Button>("MarginContainer/VBoxContainer/CenterContainer/VBoxContainer/ExitGameButton");
            exitToMainMenuButton = GetNode<Button>("MarginContainer/VBoxContainer/CenterContainer/VBoxContainer/ExitToMainMenuButton");

            saveTurnButton.Connect("pressed", this, nameof(OnSaveTurnButtonPressed));
            generateTurnButton.Connect("pressed", this, nameof(OnGenerateTurnButtonPressed));
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
                turnGenerationStatus.Visible = GameInfo != null;
                saveTurnButton.Visible = Me != null;
                saveTurnButton.Disabled = Me == null || !Me.Dirty;
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
            saveTurnButton.Disabled = Me == null || !Me.Dirty;
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
            EventManager.PublishSaveScreenshotEvent();
            EventManager.PublishPlayerDirtyEvent();
            await GamesManager.Instance.SavePlayer(GameInfo, Me);
            Me.Dirty = false;
        }

        void OnGenerateTurnButtonPressed()
        {
            if (GameInfo != null && !GameInfo.AllPlayersSubmitted())
            {
                CSConfirmDialog.Show("Some players have not submitted their turns, are you sure you want to force turn generation?",
                () =>
                {
                    EventManager.PublishGenerateTurnRequestedEvent(GameInfo);
                });
            }
        }

        void OnExitGameButtonPressed()
        {
            if (Me != null && Me.Dirty)
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
            if (Me != null && Me.Dirty)
            {
                CSConfirmDialog.Show("You have unsaved changes to your turn, are you sure you want to exit?",
                () =>
                {
                    EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
                    ServerManager.Instance.ExitGame();
                    NetworkClient.Instance.CloseConnection();
                    GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
                });
            }
            else
            {
                EventManager.PublishGameExitingEvent(PlayersManager.GameInfo);
                ServerManager.Instance.ExitGame();
                NetworkClient.Instance.CloseConnection();
                GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
            }
        }

    }
}