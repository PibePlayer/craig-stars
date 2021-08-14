using Godot;
using System;

namespace CraigStars.Client
{
    public class PlayerStatusDialog : CSDialog
    {
        PlayerStatus playerStatus;
        public override void _Ready()
        {
            base._Ready();
            playerStatus = GetNode<PlayerStatus>("MarginContainer/VBoxContainer/HBoxContainerContent/PlayerStatus");
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (IsVisibleInTree())
            {
                playerStatus.OnVisible(GameInfo);
            }
        }

    }
}