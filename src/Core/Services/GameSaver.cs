using System;
using System.Linq;
using Godot;

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

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(Game game)
        {
            using (var directory = new Directory())
            {
                directory.MakeDirRecursive(GetSaveDirPath(game.Name, game.Year));
            }
            using (var saveGame = new File())
            {
                saveGame.Open(GetSaveGamePath(game.Name, game.Year), File.ModeFlags.Write);

                var gameJson = Serializers.SerializeGame(game);
                saveGame.StoreString(gameJson);
                saveGame.Close();

                var settings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);
                foreach (var player in game.Players)
                {
                    using (var playerSave = new File())
                    {
                        playerSave.Open(GetSaveGamePlayerPath(game.Name, game.Year, player.Num), File.ModeFlags.Write);
                        var json = Serializers.Serialize(player, settings);
                        playerSave.StoreString(json);
                        playerSave.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Load this game and players from disk
        /// </summary>
        /// <param name="game"></param>
        public Game LoadGame(String name, int year, ITechStore techStore)
        {
            Game game = null;
            using (var saveGame = new File())
            {
                var path = GetSaveGamePath(name, year);
                if (!saveGame.FileExists(path))
                {
                    return null;
                }
                saveGame.Open(path, File.ModeFlags.Read);
                var gameJson = saveGame.GetAsText();

                game = Serializers.DeserializeGame(gameJson, techStore);
                var settings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);
                foreach (var player in game.Players)
                {
                    using (var playerSave = new File())
                    {
                        playerSave.Open(GetSaveGamePlayerPath(name, year, player.Num), File.ModeFlags.Read);
                        var json = playerSave.GetAsText();
                        Serializers.PopulatePlayer(json, player, settings);
                        player.SetupMapObjectMappings();
                        player.ComputeAggregates();
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
        public int GetLatestGameYear(String name)
        {
            return 2400;
        }


    }
}