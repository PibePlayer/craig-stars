using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Utils;
using Godot;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Manages listing/loading/saving games
    /// </summary>
    public class GamesManager : Node, IGamesManager
    {
        static ILog log = LogManager.GetLogger(typeof(GamesManager));

        Dictionary<Game, GameSerializer> GameSerializerByGame = new Dictionary<Game, GameSerializer>();

        private static GamesManager instance;
        public static GamesManager Instance
        {
            get
            {
                return instance;
            }
        }

        GamesManager()
        {
            instance = this;
        }

        public override void _Ready()
        {
            if (instance != this)
            {
                log.Warn("Godot created our singleton twice");
                instance = this;
            }
        }

        public static string SaveDirPath { get => $"user://saves"; }

        public static string GetSaveGameFolder(string gameName)
        {
            return $"{SaveDirPath}/{gameName}";
        }

        public static string GetSaveGameYearFolder(string gameName, int year)
        {
            return $"{GetSaveGameFolder(gameName)}/{year}";
        }

        public static string GetSaveGameFile(string gameName, int year)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/game.json";
        }

        public static string GetSaveGamePlayerPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/game-player-{playerNum}.json";
        }

        public static string GetSavePlayerPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/player-{playerNum}.json";
        }

        /// <summary>
        /// Get a list of all game folders
        /// </summary>
        /// <returns>A list of game folders</returns>
        public List<string> GetSavedGames()
        {
            List<string> gameFolders = new List<string>();

            using (var directory = new Directory())
            {
                directory.Open(SaveDirPath);
                directory.ListDirBegin(skipNavigational: true, skipHidden: true);
                while (true)
                {
                    string file = directory.GetNext();
                    if (file == null || file.Empty())
                    {
                        break;
                    }
                    if (directory.CurrentIsDir())
                    {
                        gameFolders.Add(file);
                    }
                }
            }

            gameFolders.Sort();
            return gameFolders;
        }

        /// <summary>
        /// Get a list of all year folders for a game
        /// </summary>
        /// <returns>A list of year folders for a game</returns>
        public List<int> GetSavedGameYears(string gameName)
        {
            List<int> gameYears = new List<int>();
            using (var directory = new Directory())
            {
                directory.Open(GetSaveGameFolder(gameName));
                directory.ListDirBegin(skipHidden: true);
                while (true)
                {
                    string file = directory.GetNext();
                    if (file == null || file.Empty())
                    {
                        break;
                    }
                    if (directory.CurrentIsDir() && int.TryParse(file, out var year))
                    {
                        gameYears.Add(year);
                    }
                }
            }

            gameYears.Sort();
            return gameYears;
        }


        /// <summary>
        /// Return true if a game with this name exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool GameExists(string gameName)
        {
            using (var directory = new Directory())
            {
                return directory.FileExists(GetSaveGameFolder(gameName));
            }
        }

        /// <summary>
        /// Return true if a game with this name exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool Exists(string path)
        {
            using (var directory = new Directory())
            {
                return directory.FileExists(path);
            }
        }

        public void DeleteGame(string gameName)
        {
            using (var gameDirectory = new Directory())
            {
                var path = GetSaveGameFolder(gameName);
                if (!gameDirectory.DirExists(path))
                {
                    log.Error($"Game folder {gameName} does not exist at {path}");
                    return;
                }
                log.Info($"Deleting game from {path}");
                gameDirectory.Remove(path, true);
            }
        }

        public Game LoadGame(ITechStore techStore, string name, int year = -1)
        {
            var game = new Game() { TechStore = techStore, GamesManager = this, Name = name };

            using (var saveGame = new File())
            {
                var path = GetSaveGameFile(name, year);
                if (!saveGame.FileExists(path))
                {
                    return null;
                }
                saveGame.Open(path, File.ModeFlags.Read);
                var gameJson = saveGame.GetAsText();
                saveGame.Close();

                var gameSerializerSettings = Serializers.CreateGameSettings(game);
                Serializers.PopulateGame(gameJson, game, gameSerializerSettings);
                var settings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);
                foreach (var player in game.Players)
                {
                    using (var playerSave = new File())
                    {
                        playerSave.Open(GetSaveGamePlayerPath(name, year, player.Num), File.ModeFlags.Read);
                        var json = playerSave.GetAsText();
                        playerSave.Close();
                        Serializers.PopulatePlayer(json, player, settings);
                    }
                }
            }

            return game;
        }

        public GameJson SerializeGame(Game game)
        {
            // serializers are expensive to create, so store them for later
            GameSerializer gameSerializer;
            if (!GameSerializerByGame.TryGetValue(game, out gameSerializer))
            {
                gameSerializer = new GameSerializer(game);
                GameSerializerByGame[game] = gameSerializer;
            }
            return gameSerializer.SerializeGame(game);
        }

        /// <summary>
        /// Save a game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(Game game)
        {
            SaveGame(SerializeGame(game));
        }

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(GameJson gameJson)
        {
            var dirPath = GetSaveGameYearFolder(gameJson.Name, gameJson.Year);
            log.Info($"Saving game {gameJson.Year}:{gameJson.Name} to {dirPath}");

            // create the new year directory
            using (var directory = new Directory())
            {
                directory.MakeDirRecursive(dirPath);
            }

            // queue up the various disk tasks
            var saveTasks = new List<Task>();
            saveTasks.Add(Task.Factory.StartNew(() =>
            {
                var saveGame = new File();
                saveGame.Open(GetSaveGameFile(gameJson.Name, gameJson.Year), File.ModeFlags.Write);

                saveGame.StoreString(gameJson.Game);
                saveGame.Close();
            }));

            for (int i = 0; i < gameJson.Players.Length; i++)
            {
                // all these saves can happen in parallel
                var playerJson = gameJson.Players[i];
                var playerNum = i;
                saveTasks.Add(Task.Factory.StartNew(() =>
                {
                    var playerSave = new File();
                    playerSave.Open(GetSaveGamePlayerPath(gameJson.Name, gameJson.Year, playerNum), File.ModeFlags.Write);
                    playerSave.StoreString(playerJson);
                    playerSave.Close();
                }));
            }

            Task.WaitAll(saveTasks.ToArray());
            log.Info($"{gameJson.Year}:{gameJson.Name} saved to {dirPath}");
        }

        public string SerializePlayer(Player player)
        {
            return Serializers.Serialize(player, Serializers.CreatePlayerSettings(player.Game.Players, TechStore.Instance));
        }

        public void SavePlayer(Player player)
        {
            using (var playerSave = new File())
            {
                playerSave.Open(GetSavePlayerPath(player.Game.Name, player.Game.Year, player.Num), File.ModeFlags.Write);
                playerSave.StoreString(SerializePlayer(player));
                playerSave.Close();
            }
        }

        public bool HasPlayerSave(Player player)
        {
            return Exists(GetSavePlayerPath(player.Game.Name, player.Game.Year, player.Num));
        }

        public void LoadPlayerSave(Player player)
        {
            var game = player.Game;
            using (var playerSave = new File())
            {
                playerSave.Open(GetSavePlayerPath(game.Name, game.Year, player.Num), File.ModeFlags.Read);
                var json = playerSave.GetAsText();

                Serializers.PopulatePlayer(json, player, Serializers.CreatePlayerSettings(PlayersManager.Instance.Players, TechStore.Instance));
            }
        }
    }
}