using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CraigStars
{
    // public class GuidReferenceResolver : ReferenceResolver
    // {
    //     private readonly IDictionary<Guid, MapObject> mapObjects = new Dictionary<Guid, MapObject>();

    //     public override object ResolveReference(string referenceId)
    //     {
    //         Guid guid = new Guid(referenceId);

    //         mapObjects.TryGetValue(guid, out MapObject mo);

    //         return mo;
    //     }

    //     public override string GetReference(object value, out bool alreadyExists)
    //     {
    //         MapObject mo = value as MapObject;
    //         if (mo != null)
    //         {
    //             if (!(alreadyExists = mapObjects.ContainsKey(mo.Guid)))
    //             {
    //                 mapObjects[mo.Guid] = mo;
    //             }

    //             return mo.Guid.ToString();
    //         }
    //         else
    //         {
    //             alreadyExists = false;
    //             return null;
    //         }
    //     }

    //     public override void AddReference(string reference, object value)
    //     {
    //         Guid guid = new Guid(reference);
    //         MapObject MapObject = (MapObject)value;
    //         MapObject.Guid = guid;
    //         mapObjects[guid] = MapObject;
    //     }
    // }

    /// <summary>
    /// Serializers contains a group of static methods for converting game objects to/from json
    /// </summary>
    public static class Serializers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Serializers));
        static ITraceWriter traceWriter = new MemoryTraceWriter();
        static JsonSerializerSettings simpleSettings;

        static Serializers()
        {
            simpleSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
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
        public static JsonSerializerSettings CreatePlayerSettings(List<PublicPlayerInfo> players, ITechStore techStore)
        {
            return new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TraceWriter = traceWriter,
                ContractResolver = new PlayerContractResolver<PublicPlayerInfo>(players, techStore),

                Converters = new JsonConverter[] {
                    new ColorJsonConverter(),
                    new StringEnumConverter()
                }
            };
        }

        /// <summary>
        /// Create a new JsonSerializerSettings for Game objects
        /// </summary>
        /// <param name="game">The game to create a serializer for</param>
        /// <returns></returns>
        public static JsonSerializerSettings CreateGameSettings(Game game)
        {
            return new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TraceWriter = traceWriter,
                ContractResolver = new PlayerContractResolver<Player>(game.Players, game.TechStore),

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
        static public string Serialize<T>(T item, List<PublicPlayerInfo> players, ITechStore techStore)
        {
            var json = JsonConvert.SerializeObject(item, CreatePlayerSettings(players, techStore));
            log.Debug($"Serializing {item.GetType().Name}: \n{json}");
            return json;
        }

        /// <summary>
        /// Load a player from JSON
        /// </summary>
        /// <param name="json"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public Nullable<T> Deserialize<T>(string json, List<PublicPlayerInfo> players, ITechStore techStore) where T : struct
        {
            if (json != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, CreatePlayerSettings(players, techStore));
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
        static public string SerializeGame(Game game)
        {
            try
            {
                var json = JsonConvert.SerializeObject(game, CreateGameSettings(game));
                log.Debug($"Serialized Game:\n {traceWriter.ToString()} \n{json}");
                return json;
            }
            catch (Exception e)
            {
                log.Error("Failed to serialize game into json: ", e);
            }
            finally
            {
                log.Info($"TraceWriter: \n{traceWriter.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// Load a player from JSON
        /// </summary>
        /// <param name="json"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public Game DeserializeGame(string json, ITechStore techStore)
        {
            if (json != null)
            {
                try
                {
                    Game game = new Game() { TechStore = techStore };
                    JsonConvert.PopulateObject(json, game, CreateGameSettings(game));
                    return game;
                }
                catch (Exception e)
                {
                    log.Error($"Failed to deserialize json: {json} into type: {typeof(Game)}", e);
                }

            }
            return null;

        }

        /// <summary>
        /// Load a player from JSON
        /// </summary>
        /// <param name="json"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        static public void PopulatePlayer(string json, Player player, JsonSerializerSettings settings)
        {
            try
            {
                JsonConvert.PopulateObject(json, player, settings);
            }
            catch (Exception e)
            {
                log.Error($"Failed to PopulationPlayer from json: \n{json}", e);
            }
            finally
            {
                log.Info($"TraceWriter: \n{traceWriter.ToString()}");
            }
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
        static public T DeserializeObject<T>(string json) where T : class
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
    }
}