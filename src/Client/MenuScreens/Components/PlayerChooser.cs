using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public abstract class PlayerChooser : PlayerChooser<PublicPlayerInfo>
    {
    }

    /// <summary>
    /// The PlayerChooser control is used in single player and multiplayer games to define
    /// the name, color, race, etc for a player. It has two styles. For clients joining hosts
    /// it shows only public player data. For a client showing its own player, it shows the full
    /// player data
    /// </summary>
    public abstract class PlayerChooser<T> : VBoxContainer where T : PublicPlayerInfo
    {
        /// <summary>
        /// If the player is removed, this event is triggered
        /// </summary>
        public Action<PlayerChooser<T>, T> PlayerRemovedEvent;

        public T Player { get; set; } = new Player() as T;

        /// <summary>
        /// True if this control is part of the Host's lobby
        /// </summary>
        /// <value></value>
        public bool ShowRemoveButton { get; set; } = true;

        Label playerNumLabel;
        PlayerReadyContainer playerReadyContainer;

        protected Button removePlayerButton;


        public override void _Ready()
        {
            playerNumLabel = GetNode<Label>("HBoxContainer/PlayerNumLabel");

            playerReadyContainer = GetNode<PlayerReadyContainer>("HBoxContainer/PlayerReady/PlayerReadyContainer");
            removePlayerButton = GetNode<Button>("HBoxContainer/PlayerReady/RemovePlayerButton");
            removePlayerButton.Connect("pressed", this, nameof(OnRemovePlayerButtonPressed));

            UpdateControls();
        }

        void OnRemovePlayerButtonPressed()
        {
            PlayerRemovedEvent?.Invoke(this, Player);
        }

        public virtual void UpdateControls()
        {
            if (Player != null && playerNumLabel != null)
            {
                removePlayerButton.Visible = ShowRemoveButton;
                playerNumLabel.Text = $"{Player.Num + 1}";
                playerReadyContainer.Player = Player;
                playerReadyContainer.UpdateControls();

                if (this.IsMultiplayer())
                {
                    playerReadyContainer.Player = Player;
                    playerReadyContainer.PlayerNum = Player.Num;
                    playerReadyContainer.Visible = true;
                }
                else
                {
                    playerReadyContainer.Visible = false;
                }
            }
        }
    }
}