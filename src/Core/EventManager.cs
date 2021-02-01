using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The Signals csharp class services as a way to bridge csharp and gdscript until 
    /// everything is rewritten in .net
    /// </summary>
    public static class EventManager
    {
        static ILog log = LogManager.GetLogger(typeof(EventManager));

        static EventManager()
        {
            /// configure the logger we will use
            /// TODO: this should probably be in a different function
            const string logLayoutPattern =
                "[%date %timestamp][%level][%stacktracedetail{1}] %message %newline" +
                "%exception %newline";

            var logger = (Logger)log.Logger;
            logger.Hierarchy.Root.Level = Level.All;

            var consoleAppender = new ConsoleAppender
            {
                Name = "ConsoleAppender",
                Layout = new PatternLayout(logLayoutPattern)
            };

            logger.Hierarchy.Root.AddAppender(consoleAppender);
            logger.Hierarchy.Configured = true;

            log.Info("Logging Configured");
        }

        #region Server Events

        public static event Action<Fleet> FleetBuiltEvent;

        #endregion


        #region Event Publishers

        internal static void PublishFleetBuiltEvent(Fleet fleet)
        {
            FleetBuiltEvent?.Invoke(fleet);
        }

        #endregion

    }
}
