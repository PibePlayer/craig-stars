using CraigStars.Client;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The currently active rules for this game
    /// </summary>
    public class GUIColorsProvider : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(GUIColorsProvider));

        private GUIColors colors = new GUIColors();
        public static GUIColors Colors
        {
            get
            {
                return Instance.colors;
            }
        }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static GUIColorsProvider instance;
        public static GUIColorsProvider Instance
        {
            get
            {
                return instance;
            }
        }

        GUIColorsProvider()
        {
            instance = this;
        }

        public override void _Ready()
        {
            base._Ready();

            colors = ResourceLoader.Load<GUIColors>("res://src/Client/GUIColors.tres");
        }

    }
}
