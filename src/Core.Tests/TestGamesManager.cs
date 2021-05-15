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

        public bool GameSaveFolderExists()
        {
            return true;
        }

        public List<string> GetSavedGames()
        {
            return new List<string>();
        }

        public List<int> GetSavedGameYears(string gameName)
        {
            return new List<int>();
        }

        public Game LoadGame(ITechStore techStore, ITurnProcessorManager turnProcessorManager, string name, int year = -1)
        {
            throw new System.NotImplementedException();
        }

        public void LoadPlayerSave(Player player)
        {
            throw new System.NotImplementedException();
        }

        public void SaveGame(Game game, bool multithreaded = true)
        {
        }

        public void SaveGame(GameJson gameJson, bool multithreaded = true)
        {
        }

        public void SavePlayer(Player player)
        {
        }

        public GameJson SerializeGame(Game game)
        {
            return new GameJson();
        }

        public GameJson SerializeGame(Game game, bool multithreaded = true)
        {
            throw new System.NotImplementedException();
        }
    }
}
