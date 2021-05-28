using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// The numbers for a player's score
    /// </summary>
    public class PlayerScore
    {
        public int Planets { get; set; }
        public int Starbases { get; set; }
        public int UnarmedShips { get; set; }
        public int EscortShips { get; set; }
        public int CapitalShips { get; set; }
        public int TechLevels { get; set; }
        public int Resources { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }
    }
}
