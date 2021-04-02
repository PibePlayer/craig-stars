using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    public class GameSaver
    {
        public static string GetSaveDirPath(string gameName, int year)
        {
            return $"user://saves/{gameName}/{year}";
        }

        public static string GetSaveGamePath(string gameName, int year)
        {
            return $"user://saves/{gameName}/{year}/game.json";
        }

        public static string GetSaveGamePlayerPath(string gameName, int year, int playerNum)
        {
            return $"user://saves/{gameName}/{year}/game-player-{playerNum}.json";
        }

        public static string GetSavePlayerPath(string gameName, int year, int playerNum)
        {
            return $"user://saves/{gameName}/{year}/player-{playerNum}.json";
        }

        JsonSerializerSettings gameSerializerSettings;
        JsonSerializerSettings playerSerializerSettings;
        Game game;

        public GameSaver(Game game)
        {
            this.game = game;
            playerSerializerSettings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);
            gameSerializerSettings = Serializers.CreateGameSettings(game);
        }

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public async Task SaveGame(Game game)
        {
            await Task.Factory.StartNew(() =>
            {

                using (var directory = new Directory())
                {
                    directory.MakeDirRecursive(GetSaveDirPath(game.Name, game.Year));
                }
                using (var saveGame = new File())
                {
                    saveGame.Open(GetSaveGamePath(game.Name, game.Year), File.ModeFlags.Write);

                    var gameJson = Serializers.SerializeGame(game, gameSerializerSettings);
                    saveGame.StoreString(gameJson);
                    saveGame.Close();

                    foreach (var player in game.Players)
                    {
                        using (var playerSave = new File())
                        {
                            playerSave.Open(GetSaveGamePlayerPath(game.Name, game.Year, player.Num), File.ModeFlags.Write);
                            var json = Serializers.Serialize(player, playerSerializerSettings);
                            playerSave.StoreString(json);
                            playerSave.Close();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Load this game and players from disk
        /// </summary>
        /// <param name="game"></param>
        public Game LoadGame(string name, int year, ITechStore techStore)
        {
            using (var saveGame = new File())
            {
                var path = GetSaveGamePath(name, year);
                if (!saveGame.FileExists(path))
                {
                    return null;
                }
                saveGame.Open(path, File.ModeFlags.Read);
                var gameJson = saveGame.GetAsText();
                saveGame.Close();

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

        /// <summary>
        /// Determine the latest game year from our save game folder
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetLatestGameYear(string name)
        {
            return 2400;
        }


    }
}