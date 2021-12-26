using CraigStars;
using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{
    public class Settings : Node
    {
        private static readonly string path = "user://settings.cfg";

        public event Action SettingsChangedEvent;

        #region Settings Saved to Disk

        public string PlayerName
        {
            get => playerName;
            set
            {
                playerName = value;
                config?.SetValue("network", "player_name", playerName);
                Save();
            }
        }
        string playerName = "Craig";

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

        public int ContinuePlayerNum
        {
            get => continuePlayerNum;
            set
            {
                continuePlayerNum = value;
                config?.SetValue("game", "continue_player_num", continuePlayerNum);
                Save();
            }
        }
        int continuePlayerNum = 0;

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

        /// <summary>
        /// By default we save to disk, but we disable it on the web
        /// </summary>
        /// <value></value>
        public static bool SaveToDisk
        {
            get
            {
                if (!saveToDisk.HasValue)
                {
                    try
                    {
                        saveToDisk = OS.GetName().ToLower() != "html5";
                    }
                    catch (Exception)
                    {
                        saveToDisk = true;
                    }
                }
                return saveToDisk.Value;
            }
            set
            {
                saveToDisk = value;
            }
        }
        static bool? saveToDisk;

        /// <summary>
        /// The settings to use for new or loaded games
        /// </summary>
        /// <value></value>
        public GameSettings<Player> GameSettings { get; set; } = new();
        public static void ResetGameSettings() => Instance.GameSettings = new();

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

        public override void _EnterTree()
        {
            var err = config.Load("user://settings.cfg");
            if (err == Error.Ok)
            {
                clientPort = int.Parse(config.GetValue("network", "client_port", clientPort).ToString());
                clientHost = config.GetValue("network", "client_host", clientHost).ToString();
                serverPort = int.Parse(config.GetValue("network", "server_port", serverPort).ToString());
                playerName = config.GetValue("network", "player_name", playerName).ToString();

                if (SaveToDisk && config.HasSectionKey("game", "continue_game") && config.HasSectionKey("game", "continue_year"))
                {
                    continueGame = config.GetValue("game", "continue_game", continueGame).ToString();
                    continueYear = int.Parse(config.GetValue("game", "continue_year", continueYear).ToString());
                }
                fastHotseat = config.HasSectionKey("game", "fast_hotseat")
                    && bool.Parse(config.GetValue("game", "fast_hotseat", fastHotseat).ToString());

                Save();
            }

            EventManager.GameStartedEvent += OnGameStarted;
            EventManager.TurnPassedEvent += OnTurnPassed;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameStartedEvent -= OnGameStarted;
                EventManager.TurnPassedEvent -= OnTurnPassed;
            }
        }

        void OnGameStarted(PublicGameInfo gameInfo, Player player)
        {
            if (SaveToDisk)
            {
                ContinueGame = gameInfo.Name;
                ContinueYear = gameInfo.Year;
                ContinuePlayerNum = player.Num;
            }
        }

        void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            if (SaveToDisk)
            {
                ContinueYear = gameInfo.Year;
                ContinuePlayerNum = player.Num;
            }
        }

        void Save()
        {
            config.Save(path);
            SettingsChangedEvent?.Invoke();
        }

    }
}