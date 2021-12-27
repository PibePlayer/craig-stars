using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CraigStars.Client;
using CraigStars.Utils;
using Godot;
using GodotUtils;
using GodotUtils.IO;

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

        /// <summary>
        /// Pattern matching player-1.json or player-1-game-info.json
        /// </summary>
        /// <param name="RegexOptions.IgnoreCase"></param>
        /// <returns></returns>
        readonly Regex playerSavePattern = new Regex(@"^player-(\d+)\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly Regex playerSaveGameInfoPattern = new Regex(@"^player-(\d+)-game-info\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly Regex gameSavePattern = new Regex(@"^game.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        public static string GetSavePlayerScreenshotPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/player-{playerNum}.png";
        }

        public static string GetSaveGamePlayerOrderPath(string gameName, int year, int playerNum)
        {
            return $"{GetSaveGameYearFolder(gameName, year)}/game-player-orders-{playerNum}.json";
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
                directory.Open(SaveDirPath).ThrowOnError();
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
        /// Get a list of all Player saved games
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
                directory.Open(SaveDirPath).ThrowOnError();
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
        /// <param name="gameName">The game to load</param>
        /// <returns>A list of year folders for a game</returns>
        public List<int> GetPlayerSaveYears(string gameName)
        {
            List<int> gameYears = new List<int>();

            // no years if it doesn't exist
            if (!DirExists(GetSaveGameFolder(gameName))) return gameYears;

            using (var directory = new Directory())
            {
                directory.Open(GetSaveGameFolder(gameName)).ThrowOnError();
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
                        // make sure we have player save files in this folder
                        var playerSaveFiles = ListFiles(GetSaveGameYearFolder(gameName, year), playerSavePattern);
                        var playerSaveGameInfoFiles = ListFiles(GetSaveGameYearFolder(gameName, year), playerSaveGameInfoPattern);
                        // make sure we have player saves and an equal number of player saves and player game-info files
                        if (playerSaveFiles.Count > 0 && playerSaveFiles.Count == playerSaveGameInfoFiles.Count)
                        {
                            gameYears.Add(year);
                        }
                        else
                        {
                            log.Error($"Invalid player save files in {GetSaveGameYearFolder(gameName, year)}");
                        }
                    }
                }
            }

            gameYears.Sort();
            return gameYears;
        }

        /// <summary>
        /// Get a list of all year folders for a game
        /// </summary>
        /// <param name="gameName">The game to load</param>
        /// <returns>A list of year folders for a game</returns>
        public List<int> GetGameSaveYears(string gameName)
        {
            List<int> gameYears = new List<int>();
            using (var directory = new Directory())
            {
                directory.Open(GetSaveGameFolder(gameName)).ThrowOnError();
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
                        // make sure we have player save files in this folder
                        var gameSaveFiles = ListFiles(GetSaveGameYearFolder(gameName, year), gameSavePattern);
                        // make sure we have player saves and an equal number of player saves and player game-info files
                        if (gameSaveFiles.Count > 0)
                        {
                            gameYears.Add(year);
                        }
                        else
                        {
                            log.Error($"Invalid game save files in {GetSaveGameYearFolder(gameName, year)}");
                        }
                    }
                }
            }

            gameYears.Sort();
            return gameYears;
        }

        /// <summary>
        /// Save a player screenshot for the preview page
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        /// <param name="playerNum"></param>
        /// <param name="size"></param>
        public void SavePlayerScreenshot(Viewport viewport, string gameName, int year, int playerNum, float size)
        {
            var img = viewport.GetTexture().GetData();
            img.FlipY();
            float widthRatio = size / img.GetWidth();
            img.Resize((int)size, (int)(widthRatio * img.GetHeight()));
            var path = GetSavePlayerScreenshotPath(gameName, year, playerNum);
            log.Info($"Saving screenshot to {path}");
            img.SavePng(path);
        }

        /// <summary>
        /// Get a texture for the player screenshot, or null if it doesn't exist
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public Texture GetPlayerScreenshot(string gameName, int year, int playerNum)
        {
            string path = GetSavePlayerScreenshotPath(gameName, year, playerNum);
            if (FileExists(path))
            {
                try
                {
                    var img = new Image();
                    img.Load(path).ThrowOnError();
                    var texture = new ImageTexture();
                    texture.CreateFromImage(img);
                    return texture;
                }
                catch (Exception e)
                {
                    log.Error($"Failed to load player screenshot from {path}", e);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// List all files at a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<string> ListFiles(string path, Regex pattern = null)
        {
            var files = new List<string>();
            if (!DirExists(path)) { return files; }

            using var directory = new Directory();
            directory.Open(path).ThrowOnError();
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
                    if (pattern == null || pattern.IsMatch(file))
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
        /// Return true if a files at this path exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool FileExists(string path)
        {
            using (var directory = new Directory())
            {
                return directory.FileExists(path);
            }
        }

        /// <summary>
        /// Return true if a files at this path exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool DirExists(string path)
        {
            using (var directory = new Directory())
            {
                return directory.DirExists(path);
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

            var path = GetSaveGameInfoFile(name, year);
            if (!FileExists(path))
            {
                return null;
            }

            gameInfo = Serializers.DeserializeObject<PublicGameInfo>(FileUtils.ReadFile(path));

            return gameInfo;
        }

        public List<PlayerSave> LoadPlayerSaves(string name, int year = -1)
        {
            List<PlayerSave> playerSaves = new();
            if (year == -1)
            {
                var years = GetPlayerSaveYears(name);
                year = years[years.Count - 1];
            }
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
                if (!FileExists(path))
                {
                    return null;
                }
                var gameInfoJson = FileUtils.ReadFile(path);

                // we found a save!
                playerSaves.Add(new(Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson), playerNum));
            }

            return playerSaves;
        }

        /// <summary>
        /// Validate that this player save is valid
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public bool ValidatePlayerSave(string gameName, int year, int playerNum)
        {
            return FileExists(GetSavePlayerPath(gameName, year, playerNum)) && FileExists(GetSavePlayerGameInfoPath(gameName, year, playerNum));
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
                var years = GetPlayerSaveYears(name);
                year = years[years.Count - 1];
            }
            var path = GetSavePlayerGameInfoPath(name, year, playerNum);
            if (!FileExists(path))
            {
                return null;
            }
            var gameInfoJson = FileUtils.ReadFile(path);

            gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            return gameInfo;
        }

        PublicGameInfo LoadGameInfo(string path)
        {
            PublicGameInfo gameInfo;
            if (!FileExists(path))
            {
                return null;
            }
            var gameInfoJson = FileUtils.ReadFile(path);

            gameInfo = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);

            return gameInfo;
        }

        public Game LoadGame(ITechStore techStore, string name, int year = -1)
        {

            var path = GetSaveGameFile(name, year);
            if (!FileExists(path))
            {
                return null;
            }
            var gameJson = FileUtils.ReadFile(path);

            var gameSerializerSettings = Serializers.CreateGameSettings(techStore);
            var game = Serializers.DeserializeObject<Game>(gameJson, gameSerializerSettings);

            var settings = Serializers.CreatePlayerSettings(techStore);
            for (int playerNum = 0; playerNum < game.Players.Count; playerNum++)
            {
                var json = FileUtils.ReadFile(GetSaveGamePlayerPath(name, year, playerNum));
                game.Players[playerNum] = Serializers.DeserializeObject<Player>(json, settings);

                // if we have orders for a player for this turn, load it from disk as well
                var ordersFilePath = GetSaveGamePlayerOrderPath(name, year, playerNum);
                if (FileExists(ordersFilePath))
                {
                    var ordersJson = FileUtils.ReadFile(ordersFilePath);
                    game.PlayerOrders[playerNum] = Serializers.DeserializeObject<PlayerOrders>(ordersJson, gameSerializerSettings);
                }
            }


            return game;

        }

        public GameJson SerializeGame(Game game, ITechStore techStore)
        {
            // serializers are expensive to create, so store them for later
            GameSerializer gameSerializer;
            if (!GameSerializerByGame.TryGetValue(game, out gameSerializer))
            {
                gameSerializer = new GameSerializer(game, techStore);
                GameSerializerByGame[game] = gameSerializer;
            }
            return gameSerializer.SerializeGame(game);
        }

        /// <summary>
        /// Save a game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(Game game, ITechStore techStore)
        {
            SaveGame(SerializeGame(game, techStore));
        }

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(GameJson gameJson, bool multithreaded = true)
        {
            var dirPath = GetSaveGameYearFolder(gameJson.Name, gameJson.Year);
            log.Info($"Saving game {gameJson.Year}:{gameJson.Name} to {dirPath}");

            var saveTasks = new List<Task>();
            if (multithreaded)
            {
                // queue up the various disk tasks
                saveTasks.Add(Task.Run(() =>
                {
                    FileUtils.SaveFile(GetSaveGameInfoFile(gameJson.Name, gameJson.Year), gameJson.GameInfo);
                    FileUtils.SaveFile(GetSaveGameFile(gameJson.Name, gameJson.Year), gameJson.Game);
                }));
            }
            else
            {
                FileUtils.SaveFile(GetSaveGameInfoFile(gameJson.Name, gameJson.Year), gameJson.GameInfo);
                FileUtils.SaveFile(GetSaveGameFile(gameJson.Name, gameJson.Year), gameJson.Game);
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
                        FileUtils.SaveFile(GetSaveGamePlayerPath(gameJson.Name, gameJson.Year, playerNum), playerJson);
                    }));
                }
                else
                {
                    FileUtils.SaveFile(GetSaveGamePlayerPath(gameJson.Name, gameJson.Year, playerNum), playerJson);
                }
            }

            for (int i = 0; i < gameJson.PlayerOrders.Length; i++)
            {
                // all these saves can happen in parallel
                var orderJson = gameJson.PlayerOrders[i];
                var playerNum = i;
                if (multithreaded)
                {
                    saveTasks.Add(Task.Run(() =>
                    {
                        FileUtils.SaveFile(GetSaveGamePlayerOrderPath(gameJson.Name, gameJson.Year, playerNum), orderJson);
                    }));
                }
                else
                {
                    FileUtils.SaveFile(GetSaveGamePlayerOrderPath(gameJson.Name, gameJson.Year, playerNum), orderJson);
                }
            }

            if (multithreaded)
            {
                Task.WaitAll(saveTasks.ToArray());
            }
            log.Info($"{gameJson.Year}:{gameJson.Name} saved to {dirPath}");
        }

        public string SerializePlayer(PublicGameInfo gameInfo, Player player)
        {
            return Serializers.Serialize(player, Serializers.CreatePlayerSettings(TechStore.Instance));
        }

        /// <summary>
        /// Save a player's gameinfo, i.e. player-0-game-info.json
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public async Task SavePlayerGameInfo(PublicGameInfo gameInfo, int playerNum)
        {
            // save the player's copy of the gameInfo, in case it doesn't already exist
            var playerGameInfoPath = GetSavePlayerGameInfoPath(gameInfo.Name, gameInfo.Year, playerNum);

            await Task.Run(() =>
            {
                log.Info($"Saving player {playerNum} GameInfo to {playerGameInfoPath}");
                FileUtils.SaveFile(playerGameInfoPath, Serializers.Serialize(gameInfo));
            });
        }

        /// <summary>
        /// Save a player's data and their gameInfo
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task SavePlayer(PublicGameInfo gameInfo, Player player)
        {
            var saveGameInfoTask = SavePlayerGameInfo(gameInfo, player.Num);

            await Task.Run(() =>
            {
                var path = GetSavePlayerPath(gameInfo.Name, gameInfo.Year, player.Num);
                log.Info($"Saving player {player} to {path}");
                FileUtils.SaveFile(path, SerializePlayer(gameInfo, player));
            });

            await saveGameInfoTask;
        }

        public bool HasPlayerSave(PublicGameInfo gameInfo, Player player)
        {
            return FileExists(GetSavePlayerPath(gameInfo.Name, gameInfo.Year, player.Num));
        }

        /// <summary>
        /// Get all the player saves for this game
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public List<PublicPlayerInfo> GetPlayerSaves(PublicGameInfo gameInfo)
        {
            // return all players that have save game files on disk
            return gameInfo.Players.Where(player => FileExists(GetSavePlayerPath(gameInfo.Name, gameInfo.Year, player.Num))).ToList();
        }

        /// <summary>
        /// Find the latest player save for a game
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public Player LoadPlayerSave(string gameName)
        {
            var years = GetPlayerSaveYears(gameName);
            var year = years[years.Count - 1];
            var playerSaves = ListFiles(GetSaveGameYearFolder(gameName, year), playerSavePattern);
            if (playerSaves.Count > 0)
            {
                var match = playerSavePattern.Match(playerSaves[0]);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int playerNum))
                {
                    var json = FileUtils.ReadFile(GetSavePlayerPath(gameName, year, playerNum));
                    return Serializers.DeserializeObject<Player>(json, Serializers.CreatePlayerSettings(TechStore.Instance));
                }
            }

            return null;
        }

        /// <summary>
        /// Load a player from a player save file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public Player LoadPlayerSave(PublicGameInfo gameInfo, int playerNum)
        {
            var json = FileUtils.ReadFile(GetSavePlayerPath(gameInfo.Name, gameInfo.Year, playerNum));
            return Serializers.DeserializeObject<Player>(json, Serializers.CreatePlayerSettings(TechStore.Instance));
        }
    }
}