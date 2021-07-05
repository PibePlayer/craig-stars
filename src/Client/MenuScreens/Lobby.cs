using CraigStars.Singletons;
using Godot;
using log4net;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class Lobby : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(Lobby));

        [Export]
        public PackedScene PlayerReadyContainerScene { get; set; }

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

            Signals.GameStartedEvent += OnGameStarted;
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
                child.QueueFree();
            }
            PlayersManager.Instance.Players.ForEach(p =>
            {
                AddPlayerReadyContainer(p);
            });
        }

        public override void _ExitTree()
        {
            Signals.GameStartedEvent -= OnGameStarted;
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

        /// <summary>
        /// If we are joining a server, it will send an RPC message that sends this signal to us
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarted(PublicGameInfo gameInfo)
        {
            var clientViewScene = GD.Load<PackedScene>("res://src/Client/ClientView.tscn");
            var clientView = clientViewScene.Instance<ClientView>();
            gameInfo.Players = PlayersManager.Instance.Players;
            clientView.GameInfo = gameInfo;

            // swap out our scene
            var parent = GetParent();
            parent.RemoveChild(this);
            parent.AddChild(clientView);
            QueueFree();
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
                NetworkClient.Instance.CloseConnection();
            }

            PlayersManager.Instance.Reset();
            Network.Instance.Reset();
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }

        void OnReadyButtonPressed()
        {
            var me = PlayersManager.Me;
            me.Ready = !me.Ready;

            Signals.PublishPlayerUpdatedEvent(me, notifyPeers: true);
        }

        void OnStartGameButtonPressed()
        {
            log.Info("Server: All players ready, starting the game!");
            GetTree().ChangeScene("res://src/Client/ClientView.tscn");
        }

        #endregion
    }
}