using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class UniverseGeneratorTest
    {

        [Test]
        public void TestGenerate()
        {
            var server = new Server();
            server.Init(new List<Player>() { new Player() { AIControlled = true } }, new Rules(), StaticTechStore.Instance);

            var ug = new UniverseGenerator(server);
            ug.Generate();

            Assert.AreEqual(server.Rules.NumPlanets, server.Planets.Count);
            Assert.Greater(server.Fleets.Count, 0);
            var player = server.Players[0];
            Assert.NotNull(server.Players[0].Homeworld);
            Assert.AreEqual(server.Players[0].TechLevels, new TechLevel(3, 3, 3, 3, 3, 3));
        }

    }

}