using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CraigStars
{
    /// <summary>
    /// Serializers contains a group of static methods for converting game objects to/from json
    /// </summary>
    public static class Serializers
    {
        static CSLog log = LogProvider.GetLogger(typeof(Serializers));
        static ITraceWriter traceWriter = new MemoryTraceWriter();
        static JsonSerializerSettings simpleSettings;

        static Serializers()
        {
            simpleSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                // DefaultValueHandling = DefaultValueHandling.Ignore,
                // TraceWriter = traceWriter,

                Converters = new JsonConverter[] {
                    new ColorJsonConverter(),
                    new StringEnumConverter()
                }
            };
        }

        /// <summary>
        /// Create a new JsonSerializerSettings for Player objects
        /// It is best to re-use this if possible
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public static JsonSerializerSettings CreatePlayerSettings(ITechStore techStore)
        {
            return new()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TraceWriter = traceWriter,
                ContractResolver = new TechContractResolver(techStore),

                Converters = new JsonConverter[] {
                    new ColorJsonConverter(),
                    new StringEnumConverter(),
                    new BattleRecordTokenActionConverter()
            },
            };
        }

        public static JsonSerializerSettings CreateGameSettings(ITechStore techStore)
        {
            return new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TraceWriter = traceWriter,
                ContractResolver = new TechContractResolver(techStore),

                Converters = new JsonConverter[] {
                    new ColorJsonConverter(),
                    new StringEnumConverter()
                }
            };
        }

        /// <summary>
        /// Save an item as JSON
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item, simpleSettings);
        }

        /// <summary>
        /// Save a player to JSON. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public string Serialize<T>(T item, ITechStore techStore)
        {
            var json = JsonConvert.SerializeObject(item, CreatePlayerSettings(techStore));
            // log.Debug($"Serializing {item.GetType().Name}: \n{json}");
            return json;
        }

        /// <summary>
        /// Load a player from JSON
        /// </summary>
        /// <param name="json"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public Nullable<T> Deserialize<T>(string json, ITechStore techStore) where T : struct
        {
            if (json != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, CreatePlayerSettings(techStore));
                }
                catch (Exception e)
                {
                    log.Error($"Failed to deserialize json: {json} into type: {typeof(T)}", e);
                }

            }
            return null;

        }

        /// <summary>
        /// Save a player to JSON. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public string Serialize(Player player, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(player, settings);
        }

        /// <summary>
        /// Save a game to JSON. 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        static public string SerializeGame(Game game, JsonSerializerSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(game, settings);
                return json;
            }
            catch (Exception e)
            {
                log.Error("Failed to serialize game into json: ", e);
                log.Info($"TraceWriter: \n{traceWriter.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// Load a struct from a json string. If the json fails to parse, return null
        /// </summary>
        static public Nullable<T> Deserialize<T>(string json) where T : struct
        {
            if (json != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, simpleSettings);
                }
                catch (Exception e)
                {
                    log.Error($"Failed to deserialize json: {json} into type: {typeof(T)}", e);
                }

            }
            return null;
        }

        /// <summary>
        /// Load an object from a json string. If the json fails to parse, return null
        /// </summary>
        static public T DeserializeObject<T>(string json, JsonSerializerSettings settings = null) where T : class
        {
            settings ??= simpleSettings;
            if (json != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, settings);
                }
                catch (Exception e)
                {
                    log.Error($"Failed to deserialize json: {json} into type: {typeof(T)}", e);
                }

            }
            return null;
        }

    }
}