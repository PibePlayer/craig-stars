using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class MainMenu : MarginContainer
    {
        WindowDialog hostWindow;
        WindowDialog joinWindow;
        LineEdit joinHostEdit;
        LineEdit hostPortEdit;
        LineEdit joinPortEdit;

        Control continueGameInfo;
        Button continueGameButton;
        Label continueGameNameLabel;
        SpinBox continueGameYearSpinBox;

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


            if (Settings.Instance.ContinueGame != null)
            {
                continueGameButton.Visible = continueGameInfo.Visible = true;
                continueGameNameLabel.Text = Settings.Instance.ContinueGame;
                continueGameYearSpinBox.Value = Settings.Instance.ContinueYear;
                continueGameYearSpinBox.MaxValue = Settings.Instance.ContinueYear;
                var minSizeRect = continueGameYearSpinBox.RectMinSize;
                minSizeRect.x = continueGameYearSpinBox.GetFont("").GetStringSize("2400").x;
                continueGameYearSpinBox.RectMinSize = minSizeRect;

                continueGameButton.Connect("pressed", this, nameof(OnContinueGameButtonPressed));
            }

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

            NetworkClient.Instance.PlayerUpdatedEvent += OnPlayerUpdated;
            EventManager.GameStartingEvent += OnGameStarting;
            GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
            GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NetworkClient.Instance.PlayerUpdatedEvent -= OnPlayerUpdated;
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
            NetworkClient.Instance.JoinGame(host, port);
        }

        void OnContinueGameButtonPressed()
        {
            ServerManager.Instance.ContinueGame(Settings.Instance.ContinueGame, (int)continueGameYearSpinBox.Value);
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
            ServerManager.Instance.HostGame(Settings.Instance.ServerPort);

            // join the server we just created
            CallDeferred(nameof(HostJoinNewlyHostedGame));

            this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
            {
                instance.HostMode = true;
            });
        }

        /// <summary>
        /// Join our own newly hosted game
        /// </summary>
        void HostJoinNewlyHostedGame()
        {
            NetworkClient.Instance.JoinGame("localhost", Settings.Instance.ServerPort);
        }

        public void OnServerDisconnected()
        {
            joining = false;
        }

        public void OnConnectionFailed()
        {
            joining = false;
        }

        void OnPlayerUpdated(PublicPlayerInfo player)
        {
            if (joining && this.IsClient() && player.NetworkId == this.GetNetworkId())
            {
                GetTree().ChangeScene("res://src/Client/MenuScreens/LobbyMenu.tscn");
            }
        }

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