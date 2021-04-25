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
        static CSLog log = LogProvider.GetLogger(typeof(RaceSerializerTest));
        [Test]
        public void TestSerialize()
        {
            var race = new Race()
            {
                LRTs = new HashSet<LRT>()
                {
                    LRT.ARM,
                    LRT.BET
                },
            };

            race.ResearchCost.Electronics = ResearchCostLevel.Extra;
            race.ResearchCost.Biotechnology = ResearchCostLevel.Less;

            string jsonString = Serializers.Serialize(race);
            log.Info($"{jsonString}");

            var loadedRace = Serializers.DeserializeObject<Race>(jsonString);
            Assert.AreEqual(race.Name, race.Name);
            Assert.AreEqual(ResearchCostLevel.Standard, race.ResearchCost.Energy);
            Assert.AreEqual(ResearchCostLevel.Less, race.ResearchCost.Biotechnology);
            Assert.AreEqual(ResearchCostLevel.Less, race.ResearchCost.Biotechnology);
            Assert.IsTrue(race.HasLRT(LRT.ARM));
            Assert.IsTrue(race.HasLRT(LRT.BET));
            Assert.IsFalse(race.HasLRT(LRT.OBRM));
        }

    }

}