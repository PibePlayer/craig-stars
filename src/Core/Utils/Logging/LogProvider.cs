using System;
using System.Collections;
using Godot;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.ObjectRenderer;
using log4net.Plugin;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace CraigStars
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Detect if we are running as part of a nUnit unit test.
    /// This is DIRTY and should only be used if absolutely necessary 
    /// as its usually a sign of bad design.
    /// </summary>    
    static class UnitTestDetector
    {

        private static bool _runningFromNUnit = false;

        static UnitTestDetector()
        {
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Can't do something like this as it will load the nUnit assembly
                // if (assem == typeof(NUnit.Framework.Assert))

                if (assem.FullName.ToLowerInvariant().StartsWith("nunit.framework"))
                {
                    _runningFromNUnit = true;
                    break;
                }
            }
        }

        public static bool IsRunningFromNUnit
        {
            get { return _runningFromNUnit; }
        }
    }

    public class LogProvider
    {
        public static bool UseLog4Net
        {
            get
            {
                if (!useLog4Net.HasValue)
                {
                    // nunit should never call OS.* functions
                    try
                    {
                        if (UnitTestDetector.IsRunningFromNUnit)
                        {
                            useLog4Net = true;
                        }
                    }
                    catch (Exception)
                    {
                        // ignore it, bleh
                    }

                }

                // if we aren't running in a unit test, check the OS
                if (!useLog4Net.HasValue)
                {
                    try
                    {
                        useLog4Net = OS.GetName().ToLower() != "html5";
                    }
                    catch (Exception)
                    {
                        useLog4Net = true;
                    }
                }
                return useLog4Net.Value;
            }
        }
        static bool? useLog4Net;

        // setup logging with a static initializer
        static LogProvider()
        {
            InitLogging();
        }

        static void InitLogging()
        {
            if (UseLog4Net)
            {
                var log = LogManager.GetLogger(typeof(LogProvider));
                /// configure the logger we will use
                const string logLayoutPattern =
                    "[%date %timestamp %thread][%-5level][%logger] %message %newline" +
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

                log.Info("log4net Configured");
            }
        }

        public static void Flush()
        {
            if (UseLog4Net)
            {
                LogManager.Flush(3000);
            }
        }

        public static CSLog GetLogger(Type type)
        {
            if (UseLog4Net)
            {
                return new Log4NetLog(type);
            }
            else
            {
                return new GDLog(type);
            }
        }
    }
}