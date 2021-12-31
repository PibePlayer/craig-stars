using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    public class MainMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(MainMenu));

        #region Launch Args

        /// <summary>
        /// Automatically continue the last played game
        /// </summary>
        /// <value></value>
        public bool Continue { get; set; }

        /// <summary>
        /// The Game to load or continue
        /// </summary>
        /// <value></value>
        public string GameName { get; set; }

        /// <summary>
        /// The Name of the player, if joining a new game.
        /// </summary>
        /// <value></value>
        public string PlayerName { get; set; }

        /// <summary>
        /// The year to continue, or -1 for the latest year
        /// </summary>
        /// <value></value>
        public int Year { get; set; } = -1;

        /// <summary>
        /// The server address to join
        /// </summary>
        /// <value></value>
        public string JoinServer { get; set; }

        /// <summary>
        /// The server address to join
        /// </summary>
        /// <value></value>
        public int Port { get; set; } = 3000;

        #endregion

        WindowDialog hostWindow;
        WindowDialog joinWindow;
        LineEdit joinHostEdit;
        LineEdit hostPortEdit;
        LineEdit joinPortEdit;

        Control continueGameInfo;
        Button continueGameButton;
        Label continueGameNameLabel;
        SpinBox continueGameYearSpinBox;

        Player player;
        List<PlayerMessage> Messages { get; } = new List<PlayerMessage>();
        ClientRPC clientRPC;

        private bool joining = false;

        public override void _Ready()
        {
            hostWindow = GetNode<WindowDialog>("HostWindow");
            joinWindow = GetNode<WindowDialog>("JoinWindow");
            hostPortEdit = (LineEdit)hostWindow.FindNode("PortEdit");
            joinHostEdit = (LineEdit)joinWindow.FindNode("HostEdit");
            joinPortEdit = (LineEdit)joinWindow.FindNode("PortEdit");

            continueGameInfo = (Control)FindNode("ContinueGameInfo");
            continueGameButton = (Button)FindNode("ContinueGameButton");
            continueGameNameLabel = (Label)FindNode("ContinueGameNameLabel");
            continueGameYearSpinBox = (SpinBox)FindNode("ContinueGameYearSpinBox");

            hostPortEdit.Text = Settings.Instance.ServerPort.ToString();
            joinHostEdit.Text = Settings.Instance.ClientHost;
            joinPortEdit.Text = Settings.Instance.ClientPort.ToString();

            ((CSButton)FindNode("NewGameButton")).GrabFocus();
            ((CSButton)FindNode("ExitButton")).OnPressed((b) => GetTree().Quit());
            ((CSButton)FindNode("SettingsButton")).OnPressed((b) => GetTree().ChangeScene("res://src/Client/MenuScreens/SettingsMenu.tscn"));
            ((CSButton)FindNode("NewGameButton")).OnPressed((b) => GetTree().ChangeScene("res://src/Client/MenuScreens/NewGameMenu.tscn"));
            ((CSButton)FindNode("LoadGameButton")).OnPressed((b) => GetTree().ChangeScene("res://src/Client/MenuScreens/LoadGameMenu.tscn"));
            ((CSButton)FindNode("CustomRacesButton")).OnPressed((b) => GetTree().ChangeScene("res://src/Client/MenuScreens/CustomRacesMenu.tscn"));
            FindNode("HostGameButton").Connect("pressed", this, nameof(OnHostGameButtonPressed));
            FindNode("JoinGameButton").Connect("pressed", this, nameof(OnJoinGameButtonPressed));

            joinWindow.Connect("popup_hide", this, nameof(OnJoinWindoPopupHide));
            joinWindow.FindNode("CancelButton").Connect("pressed", this, nameof(OnJoinWindowCancelButtonPressed));
            joinWindow.FindNode("JoinButton").Connect("pressed", this, nameof(OnJoinWindowJoinButtonPressed));
            hostWindow.Connect("popup_hide", this, nameof(OnHostWindowPopupHide));
            hostWindow.FindNode("HostButton").Connect("pressed", this, nameof(OnHostWindowHostButtonPressed));

            clientRPC = ClientRPC.Instance(GetTree());
            clientRPC.PlayerJoinedNewGameEvent += OnPlayerJoinedNewGame;
            clientRPC.PlayerJoinedExistingGameEvent += OnPlayerJoinedExistingGame;
            clientRPC.PlayerMessageEvent += OnPlayerMessage;

            EventManager.GameStartingEvent += OnGameStarting;
            GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
            GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));

            // setup continue game settings
            // we either get them as args to this scene from the Launcher

            if (Settings.Instance.ContinueGame != null)
            {
                continueGameButton.Visible = continueGameInfo.Visible = true;
                continueGameNameLabel.Text = Settings.Instance.ContinueGame;

                // either use the continue year from our settings, or the latest
                // valid year in case we are missing files
                var validYears = GamesManager.Instance.GetPlayerSaveYears(Settings.Instance.ContinueGame);
                if (validYears.Count > 0)
                {
                    if (!validYears.Contains(Settings.Instance.ContinueYear) && validYears.Count > 0)
                    {
                        var year = validYears[validYears.Count - 1];
                        log.Error($"Invalid ContinueYear {Settings.Instance.ContinueYear}, resetting to {year}");
                        Settings.Instance.ContinueYear = year;
                    }

                    continueGameYearSpinBox.Value = Settings.Instance.ContinueYear;
                    continueGameYearSpinBox.MaxValue = Settings.Instance.ContinueYear;


                    var minSizeRect = continueGameYearSpinBox.RectMinSize;
                    minSizeRect.x = continueGameYearSpinBox.GetFont("").GetStringSize("2400").x;
                    continueGameYearSpinBox.RectMinSize = minSizeRect;

                    continueGameButton.Connect("pressed", this, nameof(OnContinueGameButtonPressed));
                }
                else
                {
                    joining = true;
                    continueGameButton.Visible = continueGameInfo.Visible = false;
                }
            }

            // we were launched with --continue
            if (Continue)
            {
                // use the GameName and Year passed in from the launcher, or default to whatever is in settings
                GameName ??= Settings.Instance.ContinueGame;
                Year = Year == -1 ? Settings.Instance.ContinueYear : Year;

                CallDeferred(nameof(ContinueGame), GameName, Year, Settings.Instance.ContinuePlayerNum);
            }
            else if (JoinServer != null)
            {
                if (GameName != null)
                {
                    player = GamesManager.Instance.LoadPlayerSave(GameName);
                    if (player != null)
                    {
                        log.Info($"Joining existing game {GameName} on {JoinServer}:{Port} as {player}");
                        PlayersManager.Me = player;
                        PlayersManager.GameInfo = GamesManager.Instance.LoadPlayerGameInfo(GameName);
                        NetworkClient.Instance.JoinExistingGame(JoinServer, Port, player);
                    }
                    else
                    {
                        CSConfirmDialog.Show($"No local player save found for game {GameName}");
                    }
                }
                else
                {
                    log.Info($"Joining new game {JoinServer}:{Port} as {PlayerName}");
                    joining = true;
                    NetworkClient.Instance.JoinNewGame(JoinServer, Port);
                }
            }
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                clientRPC.PlayerJoinedNewGameEvent -= OnPlayerJoinedNewGame;
                clientRPC.PlayerJoinedExistingGameEvent -= OnPlayerJoinedExistingGame;
                clientRPC.PlayerMessageEvent -= OnPlayerMessage;
                EventManager.GameStartingEvent -= OnGameStarting;
            }
        }


        void OnJoinWindowCancelButtonPressed()
        {
            joining = false;
            NetworkClient.Instance.CloseConnection();
            ((Button)joinWindow.FindNode("CancelButton")).Disabled = true;
            ((Button)joinWindow.FindNode("JoinButton")).Text = "Join";
        }

        void OnJoinWindowJoinButtonPressed()
        {
            joining = true;
            ((Button)joinWindow.FindNode("CancelButton")).Disabled = false;
            ((Button)joinWindow.FindNode("JoinButton")).Text = "Joining...";
            var host = ((LineEdit)joinWindow.FindNode("HostEdit")).Text;
            var port = int.Parse(((LineEdit)joinWindow.FindNode("PortEdit")).Text);
            Settings.Instance.ClientHost = host;
            Settings.Instance.ClientPort = port;
            NetworkClient.Instance.JoinNewGame(host, port);
        }

        /// <summary>
        /// Continue a game from the menu ui when the user presses the button
        /// </summary>
        void OnContinueGameButtonPressed()
        {
            ContinueGame(Settings.Instance.ContinueGame, (int)continueGameYearSpinBox.Value, Settings.Instance.ContinuePlayerNum);
        }

        /// <summary>
        /// Continue a game
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        /// <param name="playerNum"></param>
        void ContinueGame(string gameName, int year, int playerNum)
        {
            try
            {
                var continuer = GetNode<Continuer>("Continuer");
                continuer.Continue(gameName, year, playerNum);
            }
            catch (Exception e)
            {
                log.Error($"Failed to continue game {gameName}: {year}", e);
                CSConfirmDialog.Show($"Failed to load game {gameName}: {year}");
            }

        }

        void OnHostGameButtonPressed()
        {
            // skip the host view and just go to lobby
            // we'll move the port to settings
            OnHostWindowHostButtonPressed();
            // Hide();
            // hostWindow.PopupCentered();
        }

        void OnJoinGameButtonPressed()
        {
            Hide();
            joinWindow.PopupCentered();
        }

        void OnJoinWindoPopupHide()
        {
            Show();
        }

        void OnHostWindowPopupHide()
        {
            Show();
        }

        void OnHostWindowHostButtonPressed()
        {
            PlayersManager.Reset();
            Settings.Instance.ServerPort = int.Parse(hostPortEdit.Text);
            ServerManager.Instance.HostGame(port: Settings.Instance.ServerPort);

            // join the server we just created
            CallDeferred(nameof(HostJoinNewlyHostedGame));

            this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
            {
                // we are the host, and this is a hosted multiplayer game (as opposed to dedicated server)
                instance.Hosted = true;
                instance.IsHost = true;
            });
        }

        /// <summary>
        /// Join our own newly hosted game
        /// </summary>
        void HostJoinNewlyHostedGame()
        {
            NetworkClient.Instance.JoinNewGame("localhost", Settings.Instance.ServerPort);
        }

        #region Joining Network Games

        public void OnServerDisconnected()
        {
            joining = false;
        }

        public void OnConnectionFailed()
        {
            joining = false;
        }


        void OnPlayerMessage(PlayerMessage message)
        {
            Messages.Add(message);
        }

        void OnPlayerJoinedNewGame(PublicPlayerInfo player)
        {
            // once our player is updated from the server, go to the lobby
            if (joining && this.IsClient() && player.NetworkId == this.GetNetworkId())
            {
                this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
                {
                    instance.InitialMessages.AddRange(Messages);

                    // join as a player, depending on our settings
                    var playerName = PlayerName != null ? PlayerName : Settings.Instance.PlayerName;
                    instance.PlayerName = playerName;
                });
            }
        }

        void OnPlayerJoinedExistingGame(PublicGameInfo gameInfo)
        {
            // once our player is updated from the server, go to the lobby
            if (this.IsClient())
            {
                if (player == null)
                {
                    player = GamesManager.Instance.LoadPlayerSave(gameInfo.Name);
                }

                if (player != null)
                {
                    this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
                    {
                        clientView.GameInfo = gameInfo;
                        clientView.LocalPlayers.Add(player);
                    });
                }
                else
                {
                    CSConfirmDialog.Show($"This server is running the game {gameInfo.Name}, but there are no local player saves for that game. Disconnecting.");
                    NetworkClient.Instance.CloseConnection();
                }
            }
        }

        #endregion

        /// <summary>
        /// The server will notify us when the game is ready
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarting(PublicGameInfo gameInfo)
        {
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                clientView.GameInfo = gameInfo;
            });
        }
    }
}
