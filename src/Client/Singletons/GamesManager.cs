using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CraigStars.Client;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Manages listing/loading/saving games
    /// </summary>
    public class GamesManager : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(GamesManager));

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

        public static string GetSaveGameInfoFile(string gameName, int year)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/game-info.json";
        }

        public static string GetSaveGamePlayerPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/game-player-{playerNum}.json";
        }

        public static string GetSavePlayerPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/player-{playerNum}.json";
        }

        public static string GetSavePlayerGameInfoPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/player-{playerNum}-game-info.json";
        }

        /// <summary>
        /// Get a list of all game folders
        /// </summary>
        /// <returns>A list of game folders</returns>
        public List<string> GetSavedGames()
        {
            List<string> gameFolders = new List<string>();

            if (!GameSaveFolderExists())
            {
                return gameFolders;
            }

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
        /// Get a list of all game folders
        /// </summary>
        /// <returns>A list of game folders</returns>
        public List<PlayerSave> GetPlayerSaves()
        {
            List<PlayerSave> games = new();
            List<string> gameFolders = new();

            if (!GameSaveFolderExists())
            {
                return games;
            }

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
                        games.AddRange(LoadPlayerSaves(file));
                    }
                }
            }

            gameFolders.Sort();
            return games;
        }

        /// <summary>
        /// Get a list of all year folders for a game
        /// </summary>
        /// <param name="playerSaves">True to load player years for player saves only</param>
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
                        // if (playerSaves && Exists(GetSavePlayerGameInfoPath(gameName, year, playerNum)))
                        // {
                        //     gameYears.Add(year);
                        // }
                        // else if (!playerSaves && Exists(GetSaveGameInfoFile(gameName, year)))
                        // {
                            // add all years 
                            gameYears.Add(year);
                        // }
                    }
                }
            }

            gameYears.Sort();
            return gameYears;
        }

        /// <summary>
        /// List all files at a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<string> ListFiles(string path)
        {
            List<string> files = new();
            using (var directory = new Directory())
            {
                directory.Open(path);
                directory.ListDirBegin(skipHidden: true);
                while (true)
                {
                    string file = directory.GetNext();
                    if (file == null || file.Empty())
                    {
                        break;
                    }
                    if (!directory.CurrentIsDir())
                    {
                        files.Add(file);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Return true if a game with this name exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool GameSaveFolderExists()
        {
            using (var directory = new Directory())
            {
                return directory.DirExists(SaveDirPath);
            }
        }

        /// <summary>
        /// Return true if a game with this name exists
        /// </summary>
        /// <param name="gameName">blah</param>
        /// <returns></returns>
        public bool GameExists(string gameName)
        {
            using (var directory = new Directory())
            {
                return directory.DirExists(GetSaveGameFolder(gameName));
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
            if (gameName == null && gameName.Empty())
            {
                // can't delete an empty game name (it will delete the whole save folder, very bad.)
                return;
            }
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

        public PublicGameInfo LoadServerGameInfo(string name, int year)
        {
            PublicGameInfo gameInfo;
            using (var saveGameInfo = new File())
            {
                var path = GetSaveGameInfoFile(name, year);
                if (!saveGameInfo.FileExists(path))
                {
                    return null;
                }
                saveGameInfo.Open(path, File.ModeFlags.Read);
                var gameInfoJson = saveGameInfo.GetAsText();
                saveGameInfo.Close();

                gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            }

            return gameInfo;
        }

        public List<PlayerSave> LoadPlayerSaves(string name, int year = -1)
        {
            List<PlayerSave> playerSaves = new();
            if (year == -1)
            {
                var years = GetSavedGameYears(name);
                year = years[years.Count - 1];
            }
            using (var saveGameInfo = new File())
            {
                // match player-0-game-info.json, player-1-game-info.json
                var pattern = @"player-(\d+)-game-info.json";

                List<int> playerNums = ListFiles(GetSaveGameYearFolder(name, year))
                    .Select(file =>
                    {
                        var match = Regex.Match(file, pattern);
                        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
                    })
                    .Where(playerNum => playerNum != -1)
                    .ToList();

                foreach (int playerNum in playerNums)
                {
                    var path = GetSavePlayerGameInfoPath(name, year, playerNum);
                    if (!saveGameInfo.FileExists(path))
                    {
                        return null;
                    }
                    saveGameInfo.Open(path, File.ModeFlags.Read);
                    var gameInfoJson = saveGameInfo.GetAsText();
                    saveGameInfo.Close();

                    // we found a save!
                    playerSaves.Add(new(Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson), playerNum));
                }
            }

            return playerSaves;
        }

        /// <summary>
        /// Load a player PublicGameInfo from disk.
        /// 
        /// TODO: This doesn't really work if we aren't player 0. I need to rethink this multiplayer file stuff..
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public PublicGameInfo LoadPlayerGameInfo(string name, int year = -1, int playerNum = 0)
        {
            PublicGameInfo gameInfo;
            if (year == -1)
            {
                var years = GetSavedGameYears(name);
                year = years[years.Count - 1];
            }
            using (var saveGameInfo = new File())
            {
                var path = GetSavePlayerGameInfoPath(name, year, playerNum);
                if (!saveGameInfo.FileExists(path))
                {
                    return null;
                }
                saveGameInfo.Open(path, File.ModeFlags.Read);
                var gameInfoJson = saveGameInfo.GetAsText();
                saveGameInfo.Close();

                gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            }

            return gameInfo;
        }

        PublicGameInfo LoadGameInfo(string path)
        {
            PublicGameInfo gameInfo;
            using (var saveGameInfo = new File())
            {
                if (!saveGameInfo.FileExists(path))
                {
                    return null;
                }
                saveGameInfo.Open(path, File.ModeFlags.Read);
                var gameInfoJson = saveGameInfo.GetAsText();
                saveGameInfo.Close();

                gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            }

            return gameInfo;
        }

        public Game LoadGame(ITechStore techStore, string name, int year = -1)
        {
            var game = new Game() { TechStore = techStore, Name = name };

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
                var settings = Serializers.CreatePlayerSettings(game.GameInfo.Players, game.TechStore);
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

            // after we are fully loaded, update our design and fleet aggregates
            game.ComputeAggregates();

            return game;
        }

        public GameJson SerializeGame(Game game, bool multithreaded = true)
        {
            // serializers are expensive to create, so store them for later
            GameSerializer gameSerializer;
            if (!GameSerializerByGame.TryGetValue(game, out gameSerializer))
            {
                gameSerializer = new GameSerializer(game);
                GameSerializerByGame[game] = gameSerializer;
            }
            return gameSerializer.SerializeGame(game, multithreaded);
        }

        /// <summary>
        /// Save a game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(Game game, bool multithreaded = true)
        {
            SaveGame(SerializeGame(game, multithreaded), multithreaded);
        }

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(GameJson gameJson, bool multithreaded = true)
        {
            var dirPath = GetSaveGameYearFolder(gameJson.Name, gameJson.Year);
            log.Info($"Saving game {gameJson.Year}:{gameJson.Name} to {dirPath}");

            // create the new year directory
            using (var directory = new Directory())
            {
                directory.MakeDirRecursive(dirPath);
            }

            var saveTasks = new List<Task>();
            if (multithreaded)
            {
                // queue up the various disk tasks
                saveTasks.Add(Task.Run(() =>
                {
                    var saveGameInfo = new File();
                    saveGameInfo.Open(GetSaveGameInfoFile(gameJson.Name, gameJson.Year), File.ModeFlags.Write);
                    saveGameInfo.StoreString(gameJson.GameInfo);
                    saveGameInfo.Close();

                    var saveGame = new File();
                    saveGame.Open(GetSaveGameFile(gameJson.Name, gameJson.Year), File.ModeFlags.Write);
                    saveGame.StoreString(gameJson.Game);
                    saveGame.Close();
                }));
            }
            else
            {
                var saveGameInfo = new File();
                saveGameInfo.Open(GetSaveGameInfoFile(gameJson.Name, gameJson.Year), File.ModeFlags.Write);
                saveGameInfo.StoreString(gameJson.GameInfo);
                saveGameInfo.Close();

                var saveGame = new File();
                saveGame.Open(GetSaveGameFile(gameJson.Name, gameJson.Year), File.ModeFlags.Write);
                saveGame.StoreString(gameJson.Game);
                saveGame.Close();
            }

            for (int i = 0; i < gameJson.Players.Length; i++)
            {
                // all these saves can happen in parallel
                var playerJson = gameJson.Players[i];
                var playerNum = i;
                if (multithreaded)
                {
                    saveTasks.Add(Task.Run(() =>
                    {
                        var playerSave = new File();
                        playerSave.Open(GetSaveGamePlayerPath(gameJson.Name, gameJson.Year, playerNum), File.ModeFlags.Write);
                        playerSave.StoreString(playerJson);
                        playerSave.Close();
                    }));
                }
                else
                {
                    var playerSave = new File();
                    playerSave.Open(GetSaveGamePlayerPath(gameJson.Name, gameJson.Year, playerNum), File.ModeFlags.Write);
                    playerSave.StoreString(playerJson);
                    playerSave.Close();
                }
            }

            if (multithreaded)
            {
                Task.WaitAll(saveTasks.ToArray());
            }
            log.Info($"{gameJson.Year}:{gameJson.Name} saved to {dirPath}");
        }

        public string SerializePlayer(Player player)
        {
            return Serializers.Serialize(player, Serializers.CreatePlayerSettings(player.Game.Players, TechStore.Instance));
        }

        public async void SavePlayerGameInfo(PublicGameInfo gameInfo, int playerNum)
        {
            // save the player's copy of the gameInfo, in case it doesn't already exist
            var playerGameInfoPath = GetSavePlayerGameInfoPath(gameInfo.Name, gameInfo.Year, playerNum);

            using (var playerGameInfo = new File())
            {
                playerGameInfo.Open(playerGameInfoPath, File.ModeFlags.Write);
                await Task.Run(() =>
                {
                    playerGameInfo.StoreString(Serializers.Serialize(gameInfo));
                });
                playerGameInfo.Close();
            }
        }

        public async void SavePlayer(Player player)
        {
            // save the player's copy of the gameInfo, in case it doesn't already exist
            var playerGameInfoPath = GetSavePlayerGameInfoPath(player.Game.Name, player.Game.Year, player.Num);

            using (var playerGameInfo = new File())
            {
                playerGameInfo.Open(playerGameInfoPath, File.ModeFlags.Write);
                await Task.Run(() =>
                {
                    playerGameInfo.StoreString(Serializers.Serialize(player.Game));
                });
                playerGameInfo.Close();
            }

            await Task.Run(() =>
            {
                using (var playerSave = new File())
                {
                    playerSave.Open(GetSavePlayerPath(player.Game.Name, player.Game.Year, player.Num), File.ModeFlags.Write);
                    playerSave.StoreString(SerializePlayer(player));
                    playerSave.Close();
                }
            });
        }

        public bool HasPlayerSave(Player player)
        {
            return Exists(GetSavePlayerPath(player.Game.Name, player.Game.Year, player.Num));
        }

        /// <summary>
        /// Get all the player saves for this game
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public List<PublicPlayerInfo> GetPlayerSaves(PublicGameInfo gameInfo)
        {
            // return all players that have save game files on disk
            return gameInfo.Players.Where(player => Exists(GetSavePlayerPath(gameInfo.Name, gameInfo.Year, player.Num))).ToList();
        }

        /// <summary>
        /// Load a player from a player save file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public Player LoadPlayerSave(PublicGameInfo gameInfo, int playerNum)
        {
            using (var playerSave = new File())
            {
                playerSave.Open(GetSavePlayerPath(gameInfo.Name, gameInfo.Year, playerNum), File.ModeFlags.Read);
                var json = playerSave.GetAsText();
                playerSave.Close();

                var player = new Player();
                Serializers.PopulatePlayer(json, player, Serializers.CreatePlayerSettings(gameInfo.Players, TechStore.Instance));
                return player;
            }

            throw new System.IO.InvalidDataException($"Failed to load player {playerNum} for game: {gameInfo?.Name}");
        }
    }
}