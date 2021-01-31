using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class RacePointsCalculatorTest
    {

        [Test]
        public void TestGetAdvantagePoints()
        {
            var settings = new UniverseSettings();
            RacePointsCalculator rpc = new RacePointsCalculator();
            Assert.AreEqual(25, rpc.GetAdvantagePoints(Races.Humanoid, settings.RaceStartingPoints));

            // try an invalid race with all immunities
            Race allImmuneHumanoid = Races.Humanoid;
            allImmuneHumanoid.ImmuneGrav = allImmuneHumanoid.ImmuneRad = allImmuneHumanoid.ImmuneTemp = true;
            Assert.AreEqual(-3900, rpc.GetAdvantagePoints(allImmuneHumanoid, settings.RaceStartingPoints));

            // test the builtin rabbitoids
            Assert.AreEqual(32, rpc.GetAdvantagePoints(Races.Rabbitoid, settings.RaceStartingPoints));

            // test the builtin insectoids
            Assert.AreEqual(43, rpc.GetAdvantagePoints(Races.Insectoid, settings.RaceStartingPoints));

            Race allImmuneInsectoid = Races.Insectoid;
            allImmuneInsectoid.ImmuneRad = allImmuneInsectoid.ImmuneTemp = true;
            Assert.AreEqual(-2112, rpc.GetAdvantagePoints(allImmuneInsectoid, settings.RaceStartingPoints));
        }

    }

}