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
        static CSLog log = LogProvider.GetLogger(typeof(GameSerializer));

        JsonSerializerSettings gameSerializerSettings;
        JsonSerializerSettings playerSerializerSettings;
        Game game;
        ITechStore techStore;

        public GameSerializer(Game game, ITechStore techStore)
        {
            this.game = game;
            this.techStore = techStore;
            playerSerializerSettings = Serializers.CreatePlayerSettings(techStore);
            gameSerializerSettings = Serializers.CreateGameSettings(techStore);
        }

        /// <summary>
        /// Serialize this game to json with multiple threads
        /// </summary>
        /// <param name="game"></param>
        public GameJson SerializeGame(Game game, bool multithreaded = true)
        {
            log.Info($"{game.Year}: Serializing game to JSON.");
            GameJson gameJson = new GameJson(game.Name, game.Year, game.Players.Count);
            var saveTasks = new List<Task>();

            if (multithreaded)
            {
                saveTasks.Add(Task.Run(() =>
                {
                    gameJson.GameInfo = Serializers.Serialize(game.GameInfo);
                    gameJson.Game = Serializers.SerializeGame(game, gameSerializerSettings);
                }));
            }
            else
            {
                gameJson.Game = Serializers.SerializeGame(game, gameSerializerSettings);
            }

            for (int i = 0; i < game.Players.Count; i++)
            {
                var player = game.Players[i];
                var playerNum = i;
                if (multithreaded)
                {
                    saveTasks.Add(Task.Run(() =>
                        {
                            log.Debug($"{game.Year} Serializing player to JSON: {playerNum}.");
                            var json = Serializers.Serialize(player, playerSerializerSettings);
                            gameJson.Players[playerNum] = json;
                        }));
                }
                else
                {
                    var json = Serializers.Serialize(player, playerSerializerSettings);
                    gameJson.Players[playerNum] = json;
                }
            }

            for (int i = 0; i < game.PlayerOrders.Length; i++)
            {
                var orders = game.PlayerOrders[i];

                // if the player has submitted orders, serialize it
                // otherwise we don't do anything
                if (orders != null)
                {
                    var playerNum = i;
                    if (multithreaded)
                    {
                        saveTasks.Add(Task.Run(() =>
                            {
                                log.Debug($"{game.Year} Serializing player orders to JSON: {playerNum}.");
                                var json = Serializers.Serialize(orders, techStore);
                                gameJson.PlayerOrders[playerNum] = json;
                            }));
                    }
                    else
                    {
                        var json = Serializers.Serialize(orders, techStore);
                        gameJson.PlayerOrders[playerNum] = json;
                    }
                }
            }

            if (multithreaded)
            {
                Task.WaitAll(saveTasks.ToArray());
            }
            return gameJson;
        }

    }
}