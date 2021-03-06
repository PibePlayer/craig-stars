using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class Lobby : MarginContainer
    {
        [Export]
        public PackedScene PlayerReadyContainerScene { get; set; }

        public Game Game { get; set; } = new Game();

        TextEdit chat;
        LineEdit chatMessage;
        Button startGameButton;
        Control playerReadyContainers;

        public override void _Ready()
        {
            chat = (TextEdit)FindNode("Chat");
            chatMessage = (LineEdit)FindNode("ChatMessage");
            startGameButton = (Button)FindNode("StartGameButton");
            playerReadyContainers = FindNode("PlayerReadyContainers") as Control;

            Signals.PreStartGameEvent += OnPreStartGame;
            Signals.PostStartGameEvent += OnPostStartGame;
            Signals.PlayerMessageEvent += OnPlayerMessage;
            Signals.PlayerUpdatedEvent += OnPlayerUpdated;

            chatMessage.Connect("text_entered", this, nameof(OnChatMessageTextEntered));
            FindNode("BackButton").Connect("pressed", this, nameof(OnBackButtonPressed));
            FindNode("ReadyButton").Connect("pressed", this, nameof(OnReadyButtonPressed));
            startGameButton.Connect("pressed", this, nameof(OnStartGameButtonPressed));

            if (this.IsServer())
            {
                startGameButton.Visible = true;
                CheckStartGameButton();
            }

            // Add any player messages when we come into the lobby
            PlayersManager.Instance.Messages.ForEach(m => AddPlayerMessage(m));

            // clear out the PlayerReadyContainers and add one per player
            foreach (Node child in playerReadyContainers.GetChildren())
            {
                playerReadyContainers.RemoveChild(child);
            }
            PlayersManager.Instance.Players.ForEach(p =>
            {
                AddPlayerReadyContainer(p);
            });
        }

        public override void _ExitTree()
        {
            Signals.PreStartGameEvent -= OnPreStartGame;
            Signals.PostStartGameEvent -= OnPostStartGame;
            Signals.PlayerMessageEvent -= OnPlayerMessage;
            Signals.PlayerUpdatedEvent -= OnPlayerUpdated;
        }

        void CheckStartGameButton()
        {
            startGameButton.Disabled = !Network.Instance.ReadyToStart;
        }

        void AddPlayerMessage(PlayerMessage message)
        {
            var player = PlayersManager.Instance.GetPlayer(message.playerNum);
            var host = player.Num == 0 ? "Host - " : "";
            chat.Text += $"\n{host}{player.Name}: {message.message}";
        }

        /// <summary>
        /// Add a new PlayerReadyContainer scene for this player
        /// </summary>
        /// <param name="p"></param>
        void AddPlayerReadyContainer(PublicPlayerInfo p)
        {
            PlayerReadyContainer playerReadyContainer = PlayerReadyContainerScene.Instance() as PlayerReadyContainer;
            playerReadyContainer.PlayerNum = p.Num;
            playerReadyContainers.AddChild(playerReadyContainer);
        }

        #region Event Handlers

        void OnChatMessageTextEntered(string newText)
        {
            RPC.Instance.SendMessage(newText);
            chatMessage.Text = "";
        }

        void OnPreStartGame(List<PublicPlayerInfo> players)
        {
            // Tell the server we are ready
            // RPC.Instance.SendReadyToStart();
        }

        /// <summary>
        /// Clients receive this event so they know to switch to the Game scene
        /// </summary>
        /// <param name="year"></param>
        void OnPostStartGame(int year)
        {
            GetTree().ChangeScene("res://src/Client/Game.tscn");
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            AddPlayerMessage(message);
        }

        void OnPlayerUpdated(PublicPlayerInfo player)
        {
            CheckStartGameButton();
        }

        void OnBackButtonPressed()
        {
            if (this.IsServer())
            {
                Network.Instance.CloseConnection();
            }
            else
            {
                Client.Instance.CloseConnection();
            }

            PlayersManager.Instance.Reset();
            Network.Instance.Reset();
            GetTree().ChangeScene("res://src/Client/Main.tscn");
        }

        void OnReadyButtonPressed()
        {
            var me = PlayersManager.Instance.Me;
            me.Ready = !me.Ready;

            Signals.PublishPlayerUpdatedEvent(me, notifyPeers: true);
        }

        void OnStartGameButtonPressed()
        {
            GD.Print("Server: All players ready, starting the game!");
            GetTree().ChangeScene("res://src/Client/Game.tscn");
        }

        #endregion
    }
}