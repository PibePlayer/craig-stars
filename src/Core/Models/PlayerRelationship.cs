
using Newtonsoft.Json;

namespace CraigStars
{
    public class PlayerRelationship
    {
        public int PlayerNum { get; set; }
        public PlayerRelation Relation { get; set; }
        public bool ShareMap { get; set; }

        [JsonConstructor]
        public PlayerRelationship(int playerNum, PlayerRelation relation, bool shareMap = false)
        {
            PlayerNum = playerNum;
            Relation = relation;
            ShareMap = shareMap;
        }
    }
}
