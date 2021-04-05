using System.Collections.Generic;
using Godot;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The currently active rules for this game
    /// </summary>
    public class RulesManager : Node
    {
        static ILog log = LogManager.GetLogger(typeof(RulesManager));

        // setup logging with a static initializer
        static RulesManager()
        {
            InitLogging();
        }

        private Rules rules = new Rules();
        public static Rules Rules
        {
            get
            {
                return Instance.rules;
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


        static void InitLogging()
        {
            /// configure the logger we will use
            /// TODO: this should probably be in a different function
            const string logLayoutPattern =
                "[%date %timestamp][%level][%class.%method] %message %newline" +
                "%exception %newline";

            var logger = (Logger)log.Logger;
            logger.Hierarchy.Root.Level = Level.Debug;

            var consoleAppender = new ConsoleAppender
            {
                Name = "ConsoleAppender",
                Layout = new PatternLayout(logLayoutPattern)
            };

            logger.Hierarchy.Root.AddAppender(consoleAppender);
            logger.Hierarchy.Configured = true;

            log.Info("Logging Configured");
        }
    }
}
