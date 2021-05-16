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

        Researcher researcher = new Researcher();

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