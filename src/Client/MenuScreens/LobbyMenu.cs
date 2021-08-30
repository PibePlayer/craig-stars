using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class LobbyMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LobbyMenu));

        public bool HostMode { get; set; }

        TextEdit chat;
        LineEdit chatMessage;
        Button startGameButton;
        Control playerReadyContainers;
        NewGameOptions newGameOptions;

        public Player me;

        List<PublicPlayerInfo> joinedPlayers = new List<PublicPlayerInfo>();
        List<PlayerChooser> playerChoosers = new List<PlayerChooser>();

        public List<PlayerMessage> InitialMessages { get; } = new List<PlayerMessage>();

        RPC rpc;

        public override void _Ready()
        {
            chat = (TextEdit)FindNode("Chat");
            chatMessage = (LineEdit)FindNode("ChatMessage");
            startGameButton = (Button)FindNode("StartGameButton");
            playerReadyContainers = (Control)FindNode("PlayerReadyContainers");
            newGameOptions = (NewGameOptions)FindNode("NewGameOptions");
            rpc = RPC.Instance(GetTree());

            rpc.PlayerMessageEvent += OnPlayerMessage;
            rpc.PlayerJoinedEvent += OnPlayerJoined;
            rpc.ServerPlayersListEvent += OnServerPlayersList;
            rpc.PlayerLeftEvent += OnPlayerLeft;
            EventManager.GameStartingEvent += OnGameStarting;
            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;
            rpc.ServerPlayerDataEvent += OnServerPlayerData;

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
                newGameOptions.Visible = true;
                startGameButton.Visible = true;
                CheckStartGameButton();
            }
            else
            {
                newGameOptions.Visible = false;
                startGameButton.Visible = false;
            }
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                rpc.PlayerMessageEvent -= OnPlayerMessage;
                rpc.PlayerJoinedEvent -= OnPlayerJoined;
                rpc.PlayerLeftEvent -= OnPlayerLeft;
                rpc.ServerPlayersListEvent -= OnServerPlayersList;
                rpc.ServerPlayerDataEvent -= OnServerPlayerData;
                EventManager.GameStartingEvent -= OnGameStarting;
                NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
            }
        }

        void OnServerPlayerData(Player player)
        {
            if (player.NetworkId == GetTree().GetNetworkUniqueId())
            {
                me = player;
                Node previousPlayerNode = (Node)playerReadyContainers.GetChildren()[player.Num];

                NewGamePlayer playerChooser = ResourceLoader.Load<PackedScene>("res://src/Client/MenuScreens/Components/NewGamePlayer.tscn").Instance<NewGamePlayer>();
                playerChooser.Player = player;

                playerReadyContainers.AddChildBelowNode(previousPlayerNode, playerChooser);
                previousPlayerNode.QueueFree();
            }
            else
            {
                log.Warn("We recieved player data for a different network player... that's not right.");
            }
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
            NewGameNetworkPlayer playerChooser = ResourceLoader.Load<PackedScene>("res://src/Client/MenuScreens/Components/NewGameNetworkPlayer.tscn").Instance<NewGameNetworkPlayer>();
            playerChooser.Player = p;
            playerReadyContainers.AddChild(playerChooser);
            playerChoosers.Add(playerChooser);
        }

        #region Event Handlers

        void OnChatMessageTextEntered(string newText)
        {
            var me = joinedPlayers.Find(player => player.NetworkId == GetTree().GetNetworkUniqueId());

            rpc.SendMessage(newText, me.Num);
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

        /// <summary>
        /// The server has sent us a list of all players in the game
        /// We add them to our PlayerReadyContainers control
        /// </summary>
        /// <param name="players"></param>
        void OnServerPlayersList(List<PublicPlayerInfo> players)
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
                if (child is PlayerChooser playerReadyContainer && playerReadyContainer.Player == player)
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
            var myPublicPlayer = joinedPlayers.Find(player => player.NetworkId == GetTree().GetNetworkUniqueId());
            if (myPublicPlayer != null)
            {
                myPublicPlayer.Ready = !myPublicPlayer.Ready;
                me.Ready = myPublicPlayer.Ready;

                NetworkClient.Instance.PublishPlayerUpdatedEvent(myPublicPlayer, notifyPeers: true, GetTree());
            }
            else
            {
                log.Error("Can't show ready status, no active player found for me in joined players list");
            }
        }

        void OnStartGameButtonPressed()
        {
            log.Info("Host: All players ready, starting the game!");

            GameSettings<Player> settings = newGameOptions.GetGameSettings();

            // Note: The server already knows about the game's players, so no need to pass
            // those along here
            rpc.SendGameStartRequested(settings);
        }

        #endregion
    }
}