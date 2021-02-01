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
    public class PlayerContractResolver : DefaultContractResolver
    {
        List<PublicPlayerInfo> players = new List<PublicPlayerInfo>();
        PlayerNumConverter playerNumConverter;
        TechNameConverter techNameConverter;

        public PlayerContractResolver(List<PublicPlayerInfo> players, ITechStore techStore)
        {
            this.players = players;
            playerNumConverter = new PlayerNumConverter(players);
            techNameConverter = new TechNameConverter(techStore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.IsSubclassOf(typeof(PublicPlayerInfo)))
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