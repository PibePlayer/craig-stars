using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PlayerNumConverter : JsonConverter<PublicPlayerInfo>
    {
        public Dictionary<int, PublicPlayerInfo> PlayersByNum { get; set; } = new Dictionary<int, PublicPlayerInfo>();
        public List<PublicPlayerInfo> Players { get; }

        public PlayerNumConverter(List<PublicPlayerInfo> players)
        {
            Players = players;
        }

        public override PublicPlayerInfo ReadJson(JsonReader reader, Type objectType, PublicPlayerInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int num = reader.ReadAsInt32().Value;
            return Players[num];
        }

        public override void WriteJson(JsonWriter writer, PublicPlayerInfo value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Num);
        }
    }
}