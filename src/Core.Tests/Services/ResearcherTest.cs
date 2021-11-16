using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Threading.Tasks;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ResearcherTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(ResearcherTest));

        Researcher researcher = new Researcher(new TestRulesProvider());

        [Test]
        public void TestResearchNextLevel()
        {
            // an empty player with no tech costs 50 resources to get tech level 1
            var player = new Player();
            var spent = researcher.ResearchNextLevel(player, 50);
            Assert.AreEqual(1, player.TechLevels.Energy);
            Assert.AreEqual(new TechLevel(energy: 50), spent);
            Assert.AreEqual(0, player.TechLevelsSpent.Energy);

            // it cost more each level you get, so 50 won't research the first level of weapons
            // it costs 10 more, so 60
            player.Researching = TechField.Weapons;
            spent = researcher.ResearchNextLevel(player, 50);
            Assert.AreEqual(0, player.TechLevels.Weapons);
            Assert.AreEqual(new TechLevel(weapons: 50), spent);

            spent = researcher.ResearchNextLevel(player, 10);
            Assert.AreEqual(1, player.TechLevels.Weapons);
            Assert.AreEqual(new TechLevel(weapons: 10), spent);
        }

        [Test]
        public void TestResearchNextLevelMax()
        {
            // an empty player with no tech costs 50 resources to get tech level 1
            var player = new Player()
            {
                TechLevels = new TechLevel(26, 26, 26, 26, 25, 24)
            };
            player.Researching = TechField.Energy;
            player.NextResearchField = NextResearchField.Electronics;

            // if we are maxed on energy and we try and research energy, it should
            // switch to the NextResearchField if possible, or the lowest if that field
            // is also maxed
            var spent = researcher.ResearchNextLevel(player, 1);
            Assert.AreEqual(TechField.Electronics, player.Researching);
            Assert.AreEqual(new TechLevel(electronics: 1), spent);

            // this will switch to the lowest field because that's all that is left
            player.TechLevels = new TechLevel(26, 26, 26, 26, 26, 24);
            player.Researching = TechField.Energy;
            player.NextResearchField = NextResearchField.SameField;
            spent = researcher.ResearchNextLevel(player, 1);
            Assert.AreEqual(TechField.Biotechnology, player.Researching);
            Assert.AreEqual(new TechLevel(biotechnology: 1), spent);
        }

        [Test]
        public void TestResearch()
        {
            // an empty player with no tech costs 50 resources to get tech level 1
            var player = new Player();
            researcher.Research(player, 50, TechField.Energy);
            Assert.AreEqual(1, player.TechLevels.Energy);
            Assert.AreEqual(0, player.TechLevelsSpent.Energy);

            player = new Player();
            researcher.Research(player, 100, TechField.Energy);
            Assert.AreEqual(1, player.TechLevels.Energy);
            Assert.AreEqual(50, player.TechLevelsSpent.Energy);

            player = new Player();
            researcher.Research(player, 150, TechField.Energy);
            Assert.AreEqual(2, player.TechLevels.Energy);
            Assert.AreEqual(10, player.TechLevelsSpent.Energy);
        }

        [Test]
        public void TestGetTotalCost()
        {
            // an empty player with no tech costs 50 resources to get tech level 1
            var player = new Player();
            Assert.AreEqual(50, researcher.GetTotalCost(player, TechField.Energy, 0));

            // compute the cost of the first tech level of some other tech when we have 10 energy already
            player.TechLevels.Energy = 10;
            Assert.AreEqual(150, researcher.GetTotalCost(player, TechField.Propulsion, 0));
            Assert.AreEqual(9970, researcher.GetTotalCost(player, TechField.Energy, 11));

            player.Race.ResearchCost.Propulsion = ResearchCostLevel.Extra;
            Assert.AreEqual(262, researcher.GetTotalCost(player, TechField.Propulsion, 0));

        }
    }
}