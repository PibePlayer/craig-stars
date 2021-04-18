using System.Collections.Generic;

namespace CraigStars.Tests
{
    /// <summary>
    /// A test instance of a GamesManager that does nothing
    /// </summary>
    public class TestGamesManager : IGamesManager
    {
        public void DeleteGame(string gameName)
        {
        }

        public bool Exists(string gameName)
        {
            return false;
        }

        public List<string> GetSavedGames()
        {
            return new List<string>();
        }

        public List<int> GetSavedGameYears(string gameName)
        {
            return new List<int>();
        }

        public Game LoadGame(ITechStore techStore, string name, int year = -1)
        {
            throw new System.NotImplementedException();
        }

        public void LoadPlayerSave(Player player)
        {
            throw new System.NotImplementedException();
        }

        public void SaveGame(Game game)
        {
        }

        public void SaveGame(GameJson gameJson)
        {
        }

        public void SavePlayer(Player player)
        {
        }

        public GameJson SerializeGame(Game game)
        {
            return new GameJson();
        }
    }
}