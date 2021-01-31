using System.Collections.Generic;
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

            string jsonString = Serializers.Serialize(race);
            // log.Debug($"{jsonString}");

            var loadedRace = Serializers.DeserializeObject<Race>(jsonString);
            Assert.AreEqual(race.Name, race.Name);
        }

    }

}