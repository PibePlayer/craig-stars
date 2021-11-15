using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class RaceServiceTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(RaceServiceTest));

        RaceService service;

        [SetUp]
        public void SetUp()
        {
            service = new RaceService(new TestRulesProvider());
        }

        [Test]
        public void TestLRTs()
        {
            // empty race
            Race race = new Race();
            race.PRT = PRT.HE; // no techlevels

            // should be the same as a default
            var spec = service.ComputeRaceSpecs(race);
            Assert.AreEqual(1, spec.EngineCostFactor);
            Assert.AreEqual(0, spec.EngineFailureRate);
            Assert.AreEqual(new TechLevel(), spec.StartingTechLevels);

            foreach (LRT lrt in Enum.GetValues(typeof(LRT)))
            {
                if (lrt == LRT.None)
                {
                    continue;
                }
                race.LRTs.Add(lrt);
            }

            spec = service.ComputeRaceSpecs(race);
            // check some specs
            Assert.AreEqual(.5f, spec.EngineCostFactor);
            Assert.AreEqual(.1f, spec.EngineFailureRate);

            // PRT HE + OBRM means we have -40% growth rate
            Assert.AreEqual(-.4f, spec.MaxPopulationOffset);
            Assert.AreEqual(new TechLevel(propulsion: 2), spec.StartingTechLevels);

        }
    }
}
