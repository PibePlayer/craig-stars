using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Used for storing json strings for a game
    /// </summary>
    public class GameJson
    {
        public GameJson() { }
        public GameJson(int numPlayers)
        {
            Players = new string[numPlayers];
        }

        public string Game { get; set; }
        public string[] Players { get; set; }
    }
}