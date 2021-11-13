using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The currently active rules for this game
    /// </summary>
    public class RulesManager : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(RulesManager));

        private Rules rules = new Rules();
        
        public static Rules Rules
        {
            get
            {
                return Instance?.rules;
            }
        }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static RulesManager instance;
        public static RulesManager Instance
        {
            get
            {
                return instance;
            }
        }

        RulesManager()
        {
            instance = this;
        }

    }
}
