using log4net;
using Newtonsoft.Json;
using System;

namespace CraigStars
{
    public class TechNameConverter : JsonConverter<Tech>
    {
        static CSLog log = LogProvider.GetLogger(typeof(TechNameConverter));
        public ITechStore TechStore { get; set; }

        public TechNameConverter(ITechStore techStore)
        {
            TechStore = techStore;
        }

        public override Tech ReadJson(JsonReader reader, Type objectType, Tech existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string name = reader.Value as String;
            var tech = TechStore.GetTechByName<Tech>(name);
            if (tech == null)
            {
                log.Error($"Unable to load tech from TechStore by name: {name}");
            }
            return tech;
        }

        public override void WriteJson(JsonWriter writer, Tech value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }
    }
}