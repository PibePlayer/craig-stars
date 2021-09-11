using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace CraigStars
{

    /// <summary>
    /// This contract resolver is for saving players. It automatically converts PublicPlayerInfo objects
    /// into whatever the reference is from the list of Players passed in (from a PlayersManager).
    /// 
    /// It also converts all techs to names and back
    /// </summary>
    public class PlayerContractResolver<T> : DefaultContractResolver where T : PublicPlayerInfo
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerContractResolver<T>));
        PlayerNumConverter<T> playerNumConverter;
        TechNameConverter techNameConverter;

        public PlayerContractResolver(List<T> players, ITechStore techStore)
        {
            playerNumConverter = new PlayerNumConverter<T>(players);
            techNameConverter = new TechNameConverter(techStore);
        }

        /// <summary>
        /// Update a player in the PlayerContractResolver with a new player reference
        /// </summary>
        /// <param name="player"></param>
        public void UpdatePlayer(T player)
        {
            playerNumConverter.UpdatePlayer(player);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Ignored && (property.PropertyType == typeof(PublicPlayerInfo) || property.PropertyType.IsSubclassOf(typeof(PublicPlayerInfo))))
            {
                property.Converter = playerNumConverter;
            }
            if (property.PropertyType.IsSubclassOf(typeof(Tech)))
            {
                property.Converter = techNameConverter;
            }
            return property;
        }
    }
}