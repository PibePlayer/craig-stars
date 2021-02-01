using Newtonsoft.Json;
using System;

namespace CraigStars
{
    public class TechNameConverter : JsonConverter<Tech>
    {
        public ITechStore TechStore { get; set; }

        public TechNameConverter(ITechStore techStore)
        {
            TechStore = techStore;
        }

        public override Tech ReadJson(JsonReader reader, Type objectType, Tech existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string name = reader.Value as String;
            return TechStore.GetTechByName<Tech>(name);
        }

        public override void WriteJson(JsonWriter writer, Tech value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }
    }
}