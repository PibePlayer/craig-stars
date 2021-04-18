using CraigStars;
using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class Settings : Node
    {
        private static readonly string path = "user://settings.cfg";

        public event Action SettingsChangedEvent;

        #region Settings Saved to Disk

        public string ContinueGame
        {
            get => continueGame;
            set
            {
                continueGame = value;
                config?.SetValue("game", "continue_game", continueGame);
                Save();
            }
        }
        string continueGame = null;

        public int ContinueYear
        {
            get => continueYear;
            set
            {
                continueYear = value;
                config?.SetValue("game", "continue_year", continueYear);
                Save();
            }
        }
        int continueYear = 2400;

        public bool FastHotseat
        {
            get => fastHotseat;
            set
            {
                fastHotseat = value;
                config?.SetValue("game", "fast_hotseat", fastHotseat);
                Save();
            }
        }
        bool fastHotseat;

        public string ClientHost
        {
            get => clientHost;
            set
            {
                clientHost = value;
                config?.SetValue("network", "client_host", clientHost);
                Save();
            }
        }
        string clientHost = "127.0.0.1";

        public int ClientPort
        {
            get => clientPort;
            set
            {
                clientPort = value;
                config?.SetValue("network", "client_port", clientPort);
                Save();
            }
        }
        int clientPort = 3000;

        public int ServerPort
        {
            get => serverPort;
            set
            {
                serverPort = value;
                config?.SetValue("network", "server_port", serverPort);
                Save();
            }
        }
        int serverPort = 3000;

        #endregion

        #region Volatile Settings

        public GameMode GameMode { get; set; } = GameMode.SinglePlayer;
        public bool ShouldContinueGame { get; set; }
        public string GameName { get; set; } = "A Barefoot Jaywalk";

        #endregion

        ConfigFile config = new ConfigFile();

        /// <summary>
        /// GameSettings is a singleton
        /// </summary>
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                return instance;
            }
        }

        Settings()
        {
            instance = this;
        }

        public override void _Ready()
        {
            var err = config.Load("user://settings.cfg");
            if (err == Error.Ok)
            {
                clientPort = int.Parse(config.GetValue("network", "client_port", clientPort).ToString());
                clientHost = config.GetValue("network", "client_host", clientHost).ToString();
                serverPort = int.Parse(config.GetValue("network", "server_port", serverPort).ToString());

                if (config.HasSectionKey("game", "continue_game") && config.HasSectionKey("game", "continue_year"))
                {
                    continueGame = config.GetValue("game", "continue_game", continueGame).ToString();
                    continueYear = int.Parse(config.GetValue("game", "continue_year", continueYear).ToString());
                }
                fastHotseat = config.HasSectionKey("game", "fast_hotseat")
                    && bool.Parse(config.GetValue("game", "fast_hotseat", fastHotseat).ToString());

                Save();
            }

            Signals.PostStartGameEvent += OnPostStartGame;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.PostStartGameEvent -= OnPostStartGame;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        private void OnTurnPassed(PublicGameInfo gameInfo)
        {
            ContinueYear = gameInfo.Year;
        }

        private void OnPostStartGame(PublicGameInfo gameInfo)
        {
            ContinueGame = gameInfo.Name;
            ContinueYear = gameInfo.Year;
        }

        void Save()
        {
            config.Save(path);
            SettingsChangedEvent?.Invoke();
        }

    }
}