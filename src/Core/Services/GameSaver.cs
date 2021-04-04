using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    public class GameSaver
    {
        static ILog log = LogManager.GetLogger(typeof(GameSaver));

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

        Game game;

        public GameSaver(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Save this game to disk
        /// </summary>
        /// <param name="game"></param>
        public void SaveGame(GameJson gameJson)
        {
            var dirPath = GetSaveDirPath(game.Name, game.Year);
            log.Info($"Saving game {game.Year}:{game.Name} to {dirPath}");

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
                saveGame.Open(GetSaveGamePath(game.Name, game.Year), File.ModeFlags.Write);

                saveGame.StoreString(gameJson.Game);
                saveGame.Close();
            }));

            for (int i = 0; i < game.Players.Count; i++)
            {
                // all these saves can happen in parallel
                var playerJson = gameJson.Players[i];
                saveTasks.Add(Task.Factory.StartNew(() =>
                {
                    var playerSave = new File();
                    playerSave.Open(GetSaveGamePlayerPath(game.Name, game.Year, i), File.ModeFlags.Write);
                    playerSave.StoreString(playerJson);
                    playerSave.Close();
                }));
            }

            Task.WaitAll(saveTasks.ToArray());
            log.Info($"{game.Year}:{game.Name} saved to {dirPath}");
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