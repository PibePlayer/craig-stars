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
        static CSLog log = LogProvider.GetLogger(typeof(Program));

        public static int Main(string[] args)
        {
            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            autorun.Execute(args);
            return 0;
        }
    }
}