using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace CraigStars
{

    /// <summary>
    /// This contract resolver is for resolving techs by name into tech objects
    /// 
    /// It also converts all techs to names and back
    /// </summary>
    public class TechContractResolver : DefaultContractResolver
    {
        static CSLog log = LogProvider.GetLogger(typeof(TechContractResolver));
        TechNameConverter techNameConverter;

        public TechContractResolver(ITechStore techStore)
        {
            techNameConverter = new TechNameConverter(techStore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.IsSubclassOf(typeof(Tech)))
            {
                property.Converter = techNameConverter;
            }
            return property;
        }
    }
}