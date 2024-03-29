using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// Stores PlayerIntel about a set of Discoverable items
    /// </summary>
    public class Intel<T> where T : IDiscoverable
    {

        [JsonProperty(ItemIsReference = true)]
        public List<T> Owned { get; private set; } = new List<T>();

        [JsonProperty(ItemIsReference = true)]
        public List<T> Foriegn { get; private set; } = new List<T>();

        /// <summary>
        /// Items by Guid
        /// </summary>
        /// <typeparam name="Guid"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [JsonIgnore] public Dictionary<Guid, T> ItemsByGuid { get; set; } = new Dictionary<Guid, T>();

        [JsonIgnore] public IEnumerable<T> All { get => Owned.Concat(Foriegn); }

        /// <summary>
        /// Populate a lookup table of items by guid
        /// </summary>
        public void SetupItemsByGuid()
        {
            ItemsByGuid = All.ToGuidDictionary(item => item.Guid);
        }

        /// <summary>
        /// Clear out this intel so it can be re-populated on a new scan step
        /// </summary>
        public void Clear()
        {
            Owned.Clear();
            Foriegn.Clear();
            ItemsByGuid.Clear();
        }
    }
}