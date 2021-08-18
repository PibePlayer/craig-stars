using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class Lobby : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(Lobby));

        [Export]
        public PackedScene PlayerReadyContainerScene { get; set; }

        public bool HostMode { get; set; }

        TextEdit chat;
        LineEdit chatMessage;
        Button startGameButton;
        Control playerReadyContainers;

        List<PublicPlayerInfo> joinedPlayers = new List<PublicPlayerInfo>();

        public List<PlayerMessage> InitialMessages { get; } = new List<PlayerMessage>();

        public override void _Ready()
        {
            chat = (TextEdit)FindNode("Chat");
            chatMessage = (LineEdit)FindNode("ChatMessage");
            startGameButton = (Button)FindNode("StartGameButton");
            playerReadyContainers = FindNode("PlayerReadyContainers") as Control;

            RPC.Instance(GetTree()).PlayerMessageEvent += OnPlayerMessage;
            RPC.Instance(GetTree()).PlayerJoinedEvent += OnPlayerJoined;
            RPC.Instance(GetTree()).PlayersUpdatedEvent += OnPlayersUpdated;
            RPC.Instance(GetTree()).PlayerLeftEvent += OnPlayerLeft;
            EventManager.GameStartingEvent += OnGameStarting;
            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;

            chatMessage.Connect("text_entered", this, nameof(OnChatMessageTextEntered));
            FindNode("BackButton").Connect("pressed", this, nameof(OnBackButtonPressed));
            FindNode("ReadyButton").Connect("pressed", this, nameof(OnReadyButtonPressed));
            startGameButton.Connect("pressed", this, nameof(OnStartGameButtonPressed));

            // setup any initial messages we recieved from the server before joining
            InitialMessages.ForEach(m => AddPlayerMessage(m));

            // clear out the PlayerReadyContainers and add one per player
            foreach (Node child in playerReadyContainers.GetChildren())
            {
                playerReadyContainers.RemoveChild(child);
                child.QueueFree();
            }

            if (HostMode)
            {
                startGameButton.Visible = true;
                CheckStartGameButton();
            }
        }

        public override void _ExitTree()
        {
            RPC.Instance(GetTree()).PlayerMessageEvent -= OnPlayerMessage;
            RPC.Instance(GetTree()).PlayerJoinedEvent -= OnPlayerJoined;
            RPC.Instance(GetTree()).PlayerLeftEvent -= OnPlayerLeft;
            RPC.Instance(GetTree()).PlayersUpdatedEvent -= OnPlayersUpdated;
            EventManager.GameStartingEvent -= OnGameStarting;
            NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
        }

        void CheckStartGameButton()
        {
            startGameButton.Disabled = !(joinedPlayers.Find(p => !p.Ready) == null);
        }

        void AddPlayerMessage(PlayerMessage message)
        {
            if (message.playerNum == PlayerMessage.ServerPlayerNum)
            {
                chat.Text += $"\nServer: {message.message}";
            }
            else
            {
                var player = joinedPlayers[message.playerNum];
                var host = player.Num == 0 ? "Host - " : "";
                chat.Text += $"\n{host}{player.Name}: {message.message}";
            }
        }

        /// <summary>
        /// Add a new PlayerReadyContainer scene for this player
        /// </summary>
        /// <param name="p"></param>
        void AddPlayerReadyContainer(PublicPlayerInfo p)
        {
            PlayerReadyContainer playerReadyContainer = PlayerReadyContainerScene.Instance() as PlayerReadyContainer;
            playerReadyContainer.PlayerNum = p.Num;
            playerReadyContainer.Player = p;
            playerReadyContainers.AddChild(playerReadyContainer);
        }

        #region Event Handlers

        void OnChatMessageTextEntered(string newText)
        {
            var me = joinedPlayers.Find(player => player.NetworkId == GetTree().GetNetworkUniqueId());

            RPC.Instance(GetTree()).SendMessage(newText, me.Num);
            chatMessage.Text = "";
        }

        /// <summary>
        /// If we are joining a server, it will send an RPC message that sends this signal to us
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarting(PublicGameInfo gameInfo)
        {
            // Change to the ClientView using this new GameInfo
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                clientView.GameInfo = gameInfo;
            });
        }

        void OnPlayerMessage(PlayerMessage message)
        {
            AddPlayerMessage(message);
        }

        void OnPlayerUpdated(PublicPlayerInfo player)
        {
            var existingPlayer = joinedPlayers.Find(p => p.Num == player.Num);
            existingPlayer.Update(player);

            if (HostMode)
            {
                CheckStartGameButton();
            }
        }

        void OnPlayersUpdated(List<PublicPlayerInfo> players)
        {
            foreach (Node child in playerReadyContainers.GetChildren())
            {
                playerReadyContainers.RemoveChild(child);
                child.QueueFree();
            }

            joinedPlayers.Clear();
            joinedPlayers.AddRange(players);
            joinedPlayers.ForEach(player => AddPlayerReadyContainer(player));
        }

        void OnPlayerJoined(PublicPlayerInfo player)
        {
            joinedPlayers.Add(player);
            AddPlayerReadyContainer(player);
        }

        void OnPlayerLeft(PublicPlayerInfo player)
        {
            joinedPlayers.Remove(player);

            foreach (Node child in playerReadyContainers.GetChildren())
            {
                if (child is PlayerReadyContainer playerReadyContainer && playerReadyContainer.Player == player)
                {
                    playerReadyContainers.RemoveChild(child);
                    child.QueueFree();
                    break;
                }
            }
        }

        void OnBackButtonPressed()
        {
            ServerManager.Instance.CloseConnection();
            NetworkClient.Instance.CloseConnection();

            PlayersManager.Reset();
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }

        void OnReadyButtonPressed()
        {
            var me = joinedPlayers.Find(player => player.NetworkId == GetTree().GetNetworkUniqueId());
            if (me != null)
            {
                me.Ready = !me.Ready;

                NetworkClient.Instance.PublishPlayerUpdatedEvent(me, notifyPeers: true, GetTree());
            }
            else
            {
                log.Error("Can't show ready status, no active player found for me in joined players list");
            }
        }

        void OnStartGameButtonPressed()
        {
            log.Info("Host: All players ready, starting the game!");

            // TODO: start with settings from UI
            GameSettings<Player> settings = new GameSettings<Player>()
            {
                Name = "Network Game",
                Size = Size.Small,
                Density = Density.Normal,
            };

            // Note: The server already knows about the game's players, so no need to pass
            // those along here
            RPC.Instance(GetTree()).SendGameStartRequested(settings);
        }

        #endregion
    }
}