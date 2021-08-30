using Godot;
using System;

namespace CraigStars.Client
{

    public class NewGameNetworkPlayer : PlayerChooser
    {
        Label playerNameLabel;

        public override void _Ready()
        {
            playerNameLabel = GetNode<Label>("HBoxContainer/PlayerDetails/PlayerNameLabel");
            if (Player != null)
            {
                playerNameLabel.Text = Player.Name;
            }
            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;

            base._Ready();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
            }
        }

        void OnPlayerUpdated(PublicPlayerInfo player)
        {
            if (Player != null && player.Num == Player.Num)
            {
                Player = player;
                UpdateControls();
            }
        }

        public override void UpdateControls()
        {
            base.UpdateControls();
            if (Player != null && playerNameLabel != null)
            {
                playerNameLabel.Text = Player.Name;
            }
        }

    }
}