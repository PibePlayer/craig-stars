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
    /// <summary>
    /// Code to asynchronously serialize a game to json
    /// </summary>
    public class GameSerializer
    {
        static ILog log = LogManager.GetLogger(typeof(GameSerializer));

        JsonSerializerSettings gameSerializerSettings;
        JsonSerializerSettings playerSerializerSettings;
        Game game;

        public GameSerializer(Game game)
        {
            this.game = game;
            playerSerializerSettings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);
            gameSerializerSettings = Serializers.CreateGameSettings(game);
        }

        /// <summary>
        /// Serialize this game to json with multiple threads
        /// </summary>
        /// <param name="game"></param>
        public GameJson SerializeGame(Game game)
        {
            log.Info($"{game.Year}: Serializing game to JSON.");
            GameJson gameJson = new GameJson(game.Players.Count);
            var saveTasks = new List<Task>();
            saveTasks.Add(Task.Factory.StartNew(() =>
                {
                    gameJson.Game = Serializers.SerializeGame(game, gameSerializerSettings);
                }));


            for (int i = 0; i < game.Players.Count; i++)
            {
                var player = game.Players[i];
                var playerNum = i;
                saveTasks.Add(Task.Factory.StartNew(() =>
                    {
                        var json = Serializers.Serialize(player, playerSerializerSettings);
                        gameJson.Players[playerNum] = json;
                    }));
            }

            Task.WaitAll(saveTasks.ToArray());
            return gameJson;
        }

    }
}