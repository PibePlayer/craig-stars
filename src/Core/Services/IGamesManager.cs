using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface IGamesManager
    {
        /// <summary>
        /// Get a list of saved games 
        /// </summary>
        /// <returns>A sorted list of saved games</returns>
        List<string> GetSavedGames();

        /// <summary>
        /// Get a list of saved years for a game
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns>A sorted list of years saved for a game</returns>
        List<int> GetSavedGameYears(string gameName);

        /// <summary>
        /// Check if the user://saves folder exists
        /// </summary>
        /// <returns>true if the user://saves folder exists</returns>
        bool GameSaveFolderExists();

        /// <summary>
        /// Returns true if the given saved game exists
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        bool Exists(string gameName);

        /// <summary>
        /// Load a game from disk
        /// </summary>
        /// <param name="techStore"></param>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        Game LoadGame(ITechStore techStore, ITurnProcessorManager turnProcessorManager, string name, int year = -1);

        /// <summary>
        /// Delete a game from disk
        /// </summary>
        /// <param name="gameName"></param>
        void DeleteGame(string gameName);

        /// <summary>
        /// Serialize a game to json
        /// </summary>
        /// <param name="game"></param>
        GameJson SerializeGame(Game game, bool multithreaded = true);

        /// <summary>
        /// Save a game to disk
        /// </summary>
        /// <param name="game"></param>
        void SaveGame(Game game, bool multithreaded = true);

        /// <summary>
        /// Save a game to disk
        /// </summary>
        /// <param name="game"></param>
        void SaveGame(GameJson gameJson, bool multithreaded = true);

        /// <summary>
        /// Save a player's current state to disk 
        /// Note: This is independent of the server's data. The player can revert
        /// </summary>
        /// <param name="player"></param>
        void SavePlayer(Player player);

        /// <summary>
        /// Populate a player object from a save from disk
        /// </summary>
        /// <param name="player"></param>
        void LoadPlayerSave(Player player);

    }
}