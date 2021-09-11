using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CraigStars
{
    public class PlayerJsonSerializerSettings : JsonSerializerSettings
    {
        public PlayerContractResolver<PublicPlayerInfo> PlayerContractResolver { get; set; }

        ITraceWriter traceWriter = new MemoryTraceWriter();

        public PlayerJsonSerializerSettings(List<PublicPlayerInfo> players, ITechStore techStore)
        {
            PlayerContractResolver = new PlayerContractResolver<PublicPlayerInfo>(players, techStore);
            Formatting = Formatting.Indented;
            NullValueHandling = NullValueHandling.Ignore;
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            DefaultValueHandling = DefaultValueHandling.Ignore;
            ObjectCreationHandling = ObjectCreationHandling.Replace;
            TraceWriter = traceWriter;
            ContractResolver = PlayerContractResolver;

            Converters = new JsonConverter[] {
                    new ColorJsonConverter(),
                    new StringEnumConverter(),
                    new BattleRecordTokenActionConverter()
            };
        }

        public void UpdatePlayer(Player player)
        {
            PlayerContractResolver.UpdatePlayer(player);
        }
    }
}