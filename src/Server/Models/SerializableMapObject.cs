using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface SerializableMapObject
    {
        /// <summary>
        /// Prepare this object for serialization
        /// </summary>
        void PreSerialize();

        /// <summary>
        /// After serialization, wire up values we stored by guid
        /// </summary>
        /// <param name="mapObjectsByGuid">A dictionary of map objects, keyed by guid</param>
        void PostSerialize(Dictionary<Guid, MapObject> mapObjectsByGuid);
    }
}