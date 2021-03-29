using Godot;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CraigStars
{
    public class BattleRecordTokenActionConverter : JsonConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleRecordTokenActionConverter));

        public override bool CanConvert(Type objectType)
        {
            return typeof(BattleRecordTokenAction).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // Using a nullable bool here in case "is_album" is not present on an item
            string type = (string)jo["Type"];

            BattleRecordTokenAction item = null;
            if (type == typeof(BattleRecordTokenMove).Name)
            {
                item = new BattleRecordTokenMove();
            }
            else if (type == typeof(BattleRecordTokenFire).Name)
            {
                item = new BattleRecordTokenFire();
            }
            else if (type == typeof(BattleRecordTokenRanAway).Name)
            {
                item = new BattleRecordTokenRanAway();
            }
            else if (type == typeof(BattleRecordTokenDestroyed).Name)
            {
                item = new BattleRecordTokenDestroyed();
            }

            if (item != null)
            {
                serializer.Populate(jo.CreateReader(), item);
            }
            else
            {
                log.Error($"Failed to read BattleRecordTokenAction of type {type}");
            }

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}