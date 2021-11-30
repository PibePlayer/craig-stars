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
            Assert.AreEqual(0, spec.TechCostOffset.Count);
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
            Assert.AreEqual(-.5f, spec.TechCostOffset[TechCategory.Engine]);
            Assert.AreEqual(.1f, spec.EngineFailureRate);

            // PRT HE + OBRM means we have -40% growth rate
            Assert.AreEqual(-.4f, spec.MaxPopulationOffset);
            Assert.AreEqual(new TechLevel(propulsion: 2), spec.StartingTechLevels);

        }

        [Test]
        public void TestCosts()
        {
            var rules = new Rules(0);
            // default humanoid
            Race race = new Race();
            var spec = service.ComputeRaceSpecs(race);

            Assert.AreEqual(new Cost(resources: 5), spec.Costs[QueueItemType.Mine]);
            Assert.AreEqual(new Cost(germanium: 4, resources: 10), spec.Costs[QueueItemType.Factory]);
            Assert.AreEqual(new Cost(resources: 100), spec.Costs[QueueItemType.MineralAlchemy]);
            Assert.AreEqual(rules.DefenseCost, spec.Costs[QueueItemType.Defenses]);
            Assert.AreEqual(rules.TerraformCost, spec.Costs[QueueItemType.TerraformEnvironment]);
            Assert.AreEqual(new Cost(ironium: 110, resources: 10), spec.Costs[QueueItemType.IroniumMineralPacket]);
            Assert.AreEqual(new Cost(boranium: 110, resources: 10), spec.Costs[QueueItemType.BoraniumMineralPacket]);
            Assert.AreEqual(new Cost(germanium: 110, resources: 10), spec.Costs[QueueItemType.GermaniumMineralPacket]);
            Assert.AreEqual(new Cost(ironium: 44, boranium: 44, germanium: 44, resources: 10), spec.Costs[QueueItemType.MixedMineralPacket]);
        }

        [Test]
        public void TestCosts2()
        {
            var rules = new Rules(0);
            // default humanoid
            Race race = new Race()
            {

                MineCost = 1,
                FactoryCost = 2,
                FactoriesCostLess = true,
                PRT = PRT.PP, // cheaper packets
                LRTs = new()
                {
                    LRT.MA, // cheaper mineral alchemy
                    LRT.TT, // cheaper terraforming
                }
            };
            var spec = service.ComputeRaceSpecs(race);

            Assert.AreEqual(new Cost(resources: 1), spec.Costs[QueueItemType.Mine]);
            Assert.AreEqual(new Cost(germanium: 3, resources: 2), spec.Costs[QueueItemType.Factory]);
            Assert.AreEqual(new Cost(resources: 25), spec.Costs[QueueItemType.MineralAlchemy]);
            Assert.AreEqual(rules.DefenseCost, spec.Costs[QueueItemType.Defenses]);
            Assert.AreEqual(new Cost(resources: 70), spec.Costs[QueueItemType.TerraformEnvironment]);
            Assert.AreEqual(new Cost(ironium: 70, resources: 5), spec.Costs[QueueItemType.IroniumMineralPacket]);
            Assert.AreEqual(new Cost(boranium: 70, resources: 5), spec.Costs[QueueItemType.BoraniumMineralPacket]);
            Assert.AreEqual(new Cost(germanium: 70, resources: 5), spec.Costs[QueueItemType.GermaniumMineralPacket]);
            Assert.AreEqual(new Cost(ironium: 25, boranium: 25, germanium: 25, resources: 5), spec.Costs[QueueItemType.MixedMineralPacket]);
        }

        [Test]
        public void TestStarbaseCostFactor()
        {
            // empty race, should be default 
            Race race = new Race();
            var spec = service.ComputeRaceSpecs(race);
            Assert.AreEqual(1f, spec.StarbaseCostFactor);

            // should be .8f for AR
            race.PRT = PRT.AR;
            spec = service.ComputeRaceSpecs(race);
            Assert.AreEqual(.8f, spec.StarbaseCostFactor);

            // should still be .8f for ISB and AR
            race.PRT = PRT.AR;
            race.LRTs.Add(LRT.ISB);
            spec = service.ComputeRaceSpecs(race);
            Assert.AreEqual(.8f, spec.StarbaseCostFactor);

            // should be .8f for ISB
            race.PRT = PRT.JoaT;
            race.LRTs.Add(LRT.ISB);
            spec = service.ComputeRaceSpecs(race);
            Assert.AreEqual(.8f, spec.StarbaseCostFactor);

        }
    }
}
