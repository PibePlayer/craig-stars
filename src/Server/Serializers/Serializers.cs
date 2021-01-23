using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CraigStars
{
    public class GuidReferenceResolver : ReferenceResolver
    {
        private readonly IDictionary<Guid, MapObject> mapObjects = new Dictionary<Guid, MapObject>();

        public override object ResolveReference(string referenceId)
        {
            Guid guid = new Guid(referenceId);

            mapObjects.TryGetValue(guid, out MapObject mo);

            return mo;
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            MapObject mo = value as MapObject;
            if (mo != null)
            {
                if (!(alreadyExists = mapObjects.ContainsKey(mo.Guid)))
                {
                    mapObjects[mo.Guid] = mo;
                }

                return mo.Guid.ToString();
            }
            else
            {
                alreadyExists = false;
                return null;
            }
        }

        public override void AddReference(string reference, object value)
        {
            Guid guid = new Guid(reference);
            MapObject MapObject = (MapObject)value;
            MapObject.Guid = guid;
            mapObjects[guid] = MapObject;
        }
    }

    public static class Serializers
    {
        static JsonSerializerOptions options;

        static Serializers()
        {
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                IncludeFields = true,
                IgnoreReadOnlyProperties = true,
                ReferenceHandler = new ReferenceHandler<GuidReferenceResolver>(),

                Converters =
                {
                    new JsonStringEnumConverter(),
                    new ColorJsonConverter(),
                }
            };
        }

        /// <summary>
        /// Save player data
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        static public string SavePlayer(Player player)
        {
            player.PreSerialize();
            return JsonSerializer.Serialize(player, options);
        }

        /// <summary>
        /// Load a player from json
        /// </summary>
        /// <param name="json">The json to load</param>
        /// <param name="techStore">The player</param>
        /// <returns></returns>
        static public Player LoadPlayer(string json, ITechStore techStore)
        {
            var player = JsonSerializer.Deserialize<Player>(json, options);

            player.PostSerialize(techStore);

            return player;
        }

        /// <summary>
        /// After all players have been loaded, we need to wire up the player
        /// fields in all the map objects
        /// </summary>
        /// <param name="players"></param>
        static public void WireupPlayerFields(List<Player> players)
        {
            players.ForEach(p => p.WireupPlayerFields(players));
        }
    }
}