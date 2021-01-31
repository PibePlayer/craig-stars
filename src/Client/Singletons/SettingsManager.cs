using System.Collections.Generic;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The currently active settings for this game
    /// </summary>
    public class SettingsManager : Node
    {
        private UniverseSettings settings = new UniverseSettings();
        public static UniverseSettings Settings
        {
            get
            {
                return Instance.settings;
            }
        }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static SettingsManager instance;
        public static SettingsManager Instance
        {
            get
            {
                return instance;
            }
        }


        public override void _Ready()
        {
            instance = this;
        }

    }
}
