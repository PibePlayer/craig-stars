using System;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnitLite;

namespace CraigStars.Tests
{
    public class Program
    {
        static ILog log = LogManager.GetLogger(typeof(Program));

        public static int Main(string[] args)
        {
            InitLogging();
            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            autorun.Execute(args);
            return 0;
        }

        static void InitLogging()
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
                Layout = new PatternLayout(logLayoutPattern),
            };

            logger.Hierarchy.Root.AddAppender(consoleAppender);
            logger.Hierarchy.Configured = true;

            log.Info("Logging Configured");
        }
    }
}