using Godot;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PlayerNumConverter<T> : JsonConverter<T> where T : PublicPlayerInfo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlayerNumConverter<T>));
        public List<T> Players { get; }

        public PlayerNumConverter(List<T> players)
        {
            Players = players;
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var num = reader.Value;
            if (reader.Value != null)
            {
                log.Info($"Loading player number {num}");

                // make sure we have enough players for this player number
                int playerNum = Convert.ToInt32(num);
                if (Players.Count <= playerNum)
                {
                    for (var i = Players.Count; i <= playerNum; i++)
                    {
                        var player = new Player()
                        {
                            Num = i
                        };
                        Players.Add(player as T);
                    }
                }
                return Players[playerNum];
            }
            else
            {
                log.Info($"Player Number not found {num}");
                return Players[0];
            }
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Num);
        }
    }
}