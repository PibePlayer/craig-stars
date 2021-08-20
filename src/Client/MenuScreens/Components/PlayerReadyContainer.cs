using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{
    [Tool]
    public class PlayerReadyContainer : HBoxContainer
    {
        /// <summary>
        /// the index of the player who owns this building
        /// </summary>
        /// <value></value>
        [Export(PropertyHint.Range, "1,5")]
        public int PlayerNum
        {
            get => playerNum;
            set
            {
                playerNum = value;
                UpdateName();
            }
        }
        int playerNum = 0;

        /// <summary>
        /// the index of the player who owns this building
        /// </summary>
        /// <value></value>
        [Export]
        public bool Ready
        {
            get => ready;
            set
            {
                ready = value;
                if (readyLabel != null)
                {
                    readyLabel.Text = ready ? "Ready" : "Not Ready";
                    readyLabel.Modulate = ready && Player != null ? Player.Color : Colors.White;
                    readyCheck.Modulate = readyLabel.Modulate;
                    readyCheck.Visible = ready;
                    notReadyCheck.Visible = !ready;
                }
            }
        }
        bool ready = true;

        public PublicPlayerInfo Player { get; set; }

        TextureRect readyCheck;
        TextureRect notReadyCheck;
        TextureRect playerIcon;
        TextureRect robotIcon;
        Label readyLabel;
        Label nameLabel;

        public override void _Ready()
        {
            nameLabel = GetNode<Label>("Name");
            readyLabel = GetNode<Label>("HBoxContainer/Ready");
            readyCheck = GetNode<TextureRect>("HBoxContainer/ReadyCheck");
            notReadyCheck = GetNode<TextureRect>("HBoxContainer/NotReadyCheck");
            playerIcon = GetNode<TextureRect>("HBoxContainer/PlayerIcon");
            robotIcon = GetNode<TextureRect>("HBoxContainer/RobotIcon");

            UpdateName();
            if (!Engine.EditorHint && Player != null)
            {
                OnPlayerUpdated(Player);
            }
            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
            }
        }

        void UpdateName()
        {
            if (nameLabel != null && readyLabel != null)
            {
                var player = Player;
                if (player != null)
                {
                    Ready = player.Ready;
                    robotIcon.Visible = player.AIControlled;
                    playerIcon.Visible = !player.AIControlled;

                    var color = Player.Color;
                    nameLabel.Modulate = color;
                    readyLabel.Modulate = color;
                    if (!Engine.EditorHint)
                    {
                        var me = PlayersManager.Me?.Num == PlayerNum ? " (me)" : "";
                        nameLabel.Text = $"{Player.Name}{me}";
                    }
                    else
                    {
                        if (PlayerNum >= 0)
                        {
                            nameLabel.Text = Player.Name;
                        }
                    }
                }

            }

        }

        void OnPlayerUpdated(PublicPlayerInfo player)
        {
            if (player.Num == PlayerNum)
            {
                UpdateName();
            }
        }
    }
}