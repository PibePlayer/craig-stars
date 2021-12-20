
using Newtonsoft.Json;

namespace CraigStars
{
    public class PlayerRelationship
    {
        public PlayerRelation Relation { get; set; }
        public bool ShareMap { get; set; }

        [JsonConstructor]
        public PlayerRelationship(PlayerRelation relation, bool shareMap = false)
        {
            Relation = relation;
            ShareMap = shareMap;
        }
    }
}
