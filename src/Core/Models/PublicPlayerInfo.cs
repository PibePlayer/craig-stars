
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    // [JsonObject(IsReference = true)]
    public class PublicPlayerInfo
    {
        public int NetworkId { get; set; }
        public int Num { get; set; }
        public string Name { get; set; }
        public virtual string RaceName { get; set; }
        public virtual string RacePluralName { get; set; }
        public Boolean Host { get; set; } = false;
        public Boolean Ready { get; set; } = false;
        public Boolean AIControlled { get; set; }
        public AIDifficulty AIDifficulty { get; set; } = AIDifficulty.Normal;
        public Boolean SubmittedTurn { get; set; }
        public Color Color { get; set; } = Colors.Black;
        public PlayerScore PublicScore { get; set; } = new();
        public bool Victor { get; set; }
        public HashSet<VictoryConditionType> AchievedVictoryConditions { get; set; } = new HashSet<VictoryConditionType>();

        /// <summary>
        /// Update our data from another player info (probably from a network call)
        /// </summary>
        /// <param name="playerInfo"></param>
        public void Update(PublicPlayerInfo playerInfo)
        {
            NetworkId = playerInfo.NetworkId;
            Num = playerInfo.Num;
            RaceName = playerInfo.RaceName;
            RacePluralName = playerInfo.RacePluralName;
            Name = playerInfo.Name;
            Host = playerInfo.Host;
            Ready = playerInfo.Ready;
            AIControlled = playerInfo.AIControlled;
            SubmittedTurn = playerInfo.SubmittedTurn;
            Color = playerInfo.Color;
        }

        public override string ToString()
        {
            var networkDescription = AIControlled ? "AI Controlled" : $"NetworkId: {NetworkId}";
            return $"Player {Num} {Name} ({networkDescription})";
        }


        public override bool Equals(object obj) => this.Equals(obj as PublicPlayerInfo);

        public bool Equals(PublicPlayerInfo p)
        {
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (System.Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (Num == p.Num);
        }

        public override int GetHashCode() => Num.GetHashCode();

        public static bool operator ==(PublicPlayerInfo lhs, PublicPlayerInfo rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PublicPlayerInfo lhs, PublicPlayerInfo rhs) => !(lhs == rhs);

        public static bool operator ==(Player lhs, PublicPlayerInfo rhs) => (PublicPlayerInfo)lhs == rhs;
        public static bool operator !=(Player lhs, PublicPlayerInfo rhs) => !((PublicPlayerInfo)lhs == rhs);

        public static bool operator ==(PublicPlayerInfo lhs, Player rhs) => lhs == (PublicPlayerInfo)rhs;
        public static bool operator !=(PublicPlayerInfo lhs, Player rhs) => !(lhs == (PublicPlayerInfo)rhs);


    }
}