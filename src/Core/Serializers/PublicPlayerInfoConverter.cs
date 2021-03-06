using Godot;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PublicPlayerInfoConverter : JsonConverter<PublicPlayerInfo>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PublicPlayerInfoConverter));

        public override PublicPlayerInfo ReadJson(JsonReader reader, Type objectType, PublicPlayerInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var player = existingValue;
            if (player == null)
            {
                player = new Player();
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value;
                    if (reader.Read())
                    {
                        switch (propertyName)
                        {
                            case "Name":
                                player.Name = reader.Value as String;
                                break;
                            case "Num":
                                player.Num = Convert.ToInt32(reader.Value);
                                break;
                        }
                    }
                }
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return player;
        }

        public override void WriteJson(JsonWriter writer, PublicPlayerInfo value, JsonSerializer serializer)
        {

            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(value.Name);
            writer.WritePropertyName("Num");
            writer.WriteValue(value.Num);
            writer.WriteEndObject();
        }
    }
}