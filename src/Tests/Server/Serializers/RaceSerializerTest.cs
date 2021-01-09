using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CraigStars.Singletons;
using Godot;
using log4net;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class RaceSerializerTest
    {
        ILog log = LogManager.GetLogger(typeof(RaceSerializerTest));
        [Test]
        public void TestSerialize()
        {
            var race = new Race()
            {
                LRTs = new HashSet<LRT>()
                {
                    LRT.ARM,
                    LRT.BET
                }
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

            string jsonString = JsonSerializer.Serialize(race, options);
            log.Debug($"{jsonString}");

            var loadedRace = JsonSerializer.Deserialize<Race>(jsonString, options);
            Assert.AreEqual(race.Name, race.Name);
        }

    }

}