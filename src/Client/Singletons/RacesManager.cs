using System.Collections.Generic;
using CraigStars.Client;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Manages listing/loading/saving races
    /// </summary>
    public class RacesManager : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(RacesManager));

        private static RacesManager instance;

        public static RacesManager Instance
        {
            get
            {
                return instance;
            }
        }

        RacesManager()
        {
            instance = this;
        }

        public override void _Ready()
        {
            if (instance != this)
            {
                log.Warn("Godot created our singleton twice");
                instance = this;
            }
        }

        public static string SaveDirPath { get => $"user://races"; }

        /// <summary>
        /// Get the path to a specific race file by filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetRaceFilePath(string filename)
        {
            return $"{SaveDirPath}/{filename}.json";
        }

        /// <summary>
        /// Get a list of all race.json files, without the .json prefix
        /// </summary>
        /// <returns>A list of race filenames without .json</returns>
        public static List<string> GetRaceFiles()
        {
            List<string> raceFiles = new List<string>();

            using (var directory = new Directory())
            {
                directory.Open(SaveDirPath);
                directory.ListDirBegin(skipHidden: true);
                while (true)
                {
                    string file = directory.GetNext();
                    if (file == null || file.Empty())
                    {
                        break;
                    }
                    if (file.EndsWith(".json"))
                    {
                        raceFiles.Add(file.Replace(".json", ""));
                    }
                }
            }

            raceFiles.Sort();
            return raceFiles;
        }

        /// <summary>
        /// Return true if this file exists
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool FileExists(string filename)
        {
            using (var directory = new Directory())
            {
                return directory.FileExists(GetRaceFilePath(filename));
            }
        }

        /// <summary>
        /// Save this race to the user folder
        /// </summary>
        /// <param name="race">The race to save</param>
        /// <param name="filename">the filename without extension, i.e. Humanoids</param>
        public static void SaveRace(Race race, string filename)
        {
            using (var directory = new Directory())
            {
                directory.MakeDirRecursive(SaveDirPath);
            }

            string json = Serializers.Serialize(race);

            using (var raceFile = new File())
            {
                var path = GetRaceFilePath(filename);

                log.Info($"Saving race to {path}");
                raceFile.Open(path, File.ModeFlags.Write);
                log.Debug($"Race json {json}");
                try
                {
                    raceFile.StoreString(json);
                    Client.EventManager.PublishRaceSavedEvent(race, filename);
                }
                finally
                {
                    raceFile.Close();
                }
            }
        }

        /// <summary>
        /// Load a race from a filename
        /// </summary>
        /// <param name="filename">The race filename without the .json extension</param>
        public static Race LoadRace(string filename)
        {
            using (var raceFile = new File())
            {
                var path = GetRaceFilePath(filename);
                if (!raceFile.FileExists(path))
                {
                    log.Error($"Race file {filename} does not exist at {path}");
                    return null;
                }
                log.Debug($"Loading race from {path}");
                raceFile.Open(path, File.ModeFlags.Read);
                try
                {
                    // load the race file
                    var json = raceFile.GetAsText();
                    return Serializers.DeserializeObject<Race>(json);
                }
                finally
                {
                    raceFile.Close();
                }
            }
        }

        public static void DeleteRace(string filename)
        {
            using (var raceFile = new File())
            {
                var path = GetRaceFilePath(filename);
                if (!raceFile.FileExists(path))
                {
                    log.Error($"Race file {filename} does not exist at {path}");
                    return;
                }
                log.Info($"Deleting race from {path}");

                using (var directory = new Directory())
                {
                    directory.Remove(path);
                }
            }
        }

    }
}