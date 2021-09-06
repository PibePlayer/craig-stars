using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class LobbyMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LobbyMenu));

        public bool IsHost { get; set; }
        public string PlayerName { get; set; }

        TextEdit chat;
        LineEdit chatMessage;
        Button startGameButton;
        Button addPlayerButton;
        Control playerReadyContainers;
        NewGameOptions newGameOptions;

        Player me;


        List<PublicPlayerInfo> joinedPlayers = new List<PublicPlayerInfo>();
        List<PlayerChooser> playerChoosers = new List<PlayerChooser>();

        public List<PlayerMessage> InitialMessages { get; } = new List<PlayerMessage>();

        ServerRPC serverRPC;
        ClientRPC clientRPC;

        public override void _Ready()
        {
            chat = (TextEdit)FindNode("Chat");
            chatMessage = (LineEdit)FindNode("ChatMessage");
            startGameButton = (Button)FindNode("StartGameButton");
            playerReadyContainers = (Control)FindNode("PlayerReadyContainers");
            newGameOptions = (NewGameOptions)FindNode("NewGameOptions");
            addPlayerButton = (Button)FindNode("AddPlayerButton");
            serverRPC = ServerRPC.Instance(GetTree());
            clientRPC = ClientRPC.Instance(GetTree());

            clientRPC.PlayerMessageEvent += OnPlayerMessage;
            clientRPC.PlayerJoinedNewGameEvent += OnPlayerJoinedNewGame;
            clientRPC.ServerPlayersListEvent += OnServerPlayersList;
            clientRPC.PlayerLeftEvent += OnPlayerLeft;
            clientRPC.ServerPlayerDataEvent += OnServerPlayerData;

            EventManager.GameStartingEvent += OnGameStarting;
            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;

            chatMessage.Connect("text_entered", this, nameof(OnChatMessageTextEntered));
            FindNode("BackButton").Connect("pressed", this, nameof(OnBackButtonPressed));
            FindNode("ReadyButton").Connect("pressed", this, nameof(OnReadyButtonPressed));
            startGameButton.Connect("pressed", this, nameof(OnStartGameButtonPressed));
            addPlayerButton.Connect("pressed", this, nameof(OnAddPlayerButtonPressed));

            // setup any initial messages we received from the server before joining
            InitialMessages.ForEach(m => AddPlayerMessage(m));

            // clear out the PlayerReadyContainers and add one per player
            foreach (Node child in playerReadyContainers.GetChildren())
            {
                playerReadyContainers.RemoveChild(child);
                child.QueueFree();
            }

            if (IsHost)
            {
                newGameOptions.Visible = true;
                startGameButton.Visible = true;
                addPlayerButton.Visible = true;
                CheckStartGameButton();
            }
            else
            {
                newGameOptions.Visible = false;
                startGameButton.Visible = false;
                addPlayerButton.Visible = false;
            }

            PlayerName ??= Settings.Instance.PlayerName;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                clientRPC.PlayerMessageEvent -= OnPlayerMessage;
                clientRPC.PlayerJoinedNewGameEvent -= OnPlayerJoinedNewGame;
                clientRPC.PlayerLeftEvent -= OnPlayerLeft;
                clientRPC.ServerPlayersListEvent -= OnServerPlayersList;
                clientRPC.ServerPlayerDataEvent -= OnServerPlayerData;
                EventManager.GameStartingEvent -= OnGameStarting;
                NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
            }
        }

        void OnServerPlayerData(PublicGameInfo gameInfo, Player player)
        {
            if (player.NetworkId == GetTree().GetNetworkUniqueId() || player.AIControlled)
            {
                if (!player.AIControlled)
                {
                    me = player;
                    me.Name = PlayerName;
                }
                Node previousPlayerNode = (Node)playerReadyContainers.GetChildren()[player.Num];

                NewGamePlayer playerChooser = ResourceLoader.Load<PackedScene>("res://src/Client/MenuScreens/Components/NewGamePlayer.tscn").Instance<NewGamePlayer>();
                playerChooser.Player = player;
                playerChooser.ShowRemoveButton = player.AIControlled;
                playerChooser.PlayerRemovedEvent += OnRemoveAIPlayer;

                playerReadyContainers.AddChildBelowNode(previousPlayerNode, playerChooser);
                previousPlayerNode.QueueFree();

                // let the server know about our new name
                NetworkClient.Instance.PublishPlayerUpdatedEvent(me, notifyPeers: true, GetTree());
            }
            else
            {
                log.Warn("We received player data for a different network player... that's not right.");
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
            playerChooser.ShowRemoveButton = IsHost;
            playerChooser.PlayerRemovedEvent += OnKickPlayer;
            playerReadyContainers.AddChild(playerChooser);
            playerChoosers.Add(playerChooser);
        }

        #region Event Handlers

        void OnChatMessageTextEntered(string newText)
        {
            var me = joinedPlayers.Find(player => player.NetworkId == GetTree().GetNetworkUniqueId());

            clientRPC.SendMessage(newText, me.Num);
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

            if (IsHost)
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

        void OnPlayerJoinedNewGame(PublicPlayerInfo player)
        {
            joinedPlayers.Add(player);
            AddPlayerReadyContainer(player);
        }

        void OnPlayerLeft(PublicPlayerInfo player, List<PublicPlayerInfo> remainingPlayers)
        {
            joinedPlayers = new(remainingPlayers);

            int remainingPlayerIndex = 0;
            foreach (Node child in playerReadyContainers.GetChildren())
            {
                if (child is NewGamePlayer newGamePlayer)
                {
                    if (newGamePlayer.Player == player)
                    {
                        playerReadyContainers.RemoveChild(child);
                        child.QueueFree();
                    }
                    else
                    {
                        newGamePlayer.Player.Update(remainingPlayers[remainingPlayerIndex++]);
                        newGamePlayer.UpdateControls();
                    }
                }
                else if (child is NewGameNetworkPlayer newGameNetworkPlayer)
                {
                    if (newGameNetworkPlayer.Player == player)
                    {
                        playerReadyContainers.RemoveChild(child);
                        child.QueueFree();
                    }
                    else
                    {
                        newGameNetworkPlayer.Player = remainingPlayers[remainingPlayerIndex++];
                        newGameNetworkPlayer.UpdateControls();
                    }
                }
            }

            // update our player number if it changed
            var myNewPlayerNum = remainingPlayers.Find(p => p.NetworkId == GetTree().GetNetworkUniqueId()).Num;
            me.Num = myNewPlayerNum;


        }

        void OnRemoveAIPlayer(PlayerChooser<Player> playerChooser, Player player)
        {
            playerReadyContainers.RemoveChild(playerChooser);
            joinedPlayers.Remove(player);
            playerChooser.QueueFree();
            NetworkClient.Instance.KickPlayer(player.Num);
        }

        void OnKickPlayer(PlayerChooser<PublicPlayerInfo> playerChooser, PublicPlayerInfo player)
        {
            playerReadyContainers.RemoveChild(playerChooser);
            joinedPlayers.Remove(player);
            playerChooser.QueueFree();
            NetworkClient.Instance.KickPlayer(player.Num);
        }

        void OnAddPlayerButtonPressed()
        {
            if (IsHost)
            {
                // tell the server to add a new AI player
                NetworkClient.Instance.AddAIPlayer();
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
            settings.Mode = GameMode.NetworkedMultiPlayer;

            if (GamesManager.Instance.GameExists(settings.Name))
            {
                CSConfirmDialog.Show($"A game named {settings.Name} already exists. Are you sure you want to overwrite it?", () =>
                {
                    // delete the existing game
                    GamesManager.Instance.DeleteGame(settings.Name);
                    // Note: The server already knows about the game's players, so no need to pass
                    // those along here
                    serverRPC.SendStartNewGameRequest(settings);
                });
            }
            else
            {
                // Note: The server already knows about the game's players, so no need to pass
                // those along here
                serverRPC.SendStartNewGameRequest(settings);
            }

        }

        #endregion
    }
}