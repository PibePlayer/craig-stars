using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Used for storing json strings for a game
    /// </summary>
    public class GameJson
    {
        public GameJson() { }
        public GameJson(string name, int year, int numPlayers)
        {
            this.Name = name;
            this.Year = year;
            Players = new string[numPlayers];
            PlayerOrders = new string[numPlayers];
        }

        public string Name { get; private set; }
        public int Year { get; private set; }
        public string GameInfo { get; set; }
        public string Game { get; set; }
        public string[] Players { get; set; }
        public string[] PlayerOrders { get; set; }
    }
}