using System;
using System.Reflection;
using NUnitLite;

namespace CraigStars.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            autorun.Execute(args);
            return 0;
        }
    }
}