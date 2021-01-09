using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CraigStars
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                new Color(reader.GetString())
        ;

        public override void Write(
            Utf8JsonWriter writer,
            Color color,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(color.ToHtml())
        ;
    }
}