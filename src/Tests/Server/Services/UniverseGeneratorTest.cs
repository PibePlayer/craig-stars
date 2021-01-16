using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class UniverseGeneratorTest
    {

        [Test]
        public void TestGenerateTurn()
        {
            var server = new Server();
            server.Init(new List<Player>() { new Player() { AIControlled = true } }, new UniverseSettings(), TechStore.Instance);

            var ug = new UniverseGenerator(server);
            ug.Generate();

            var tg = new TurnGenerator(server);
            tg.GenerateTurn();
            tg.RunTurnProcessors();

            Assert.IsTrue(server.Planets.Count > 0);
            Assert.IsTrue(server.Fleets.Count > 0);
            Assert.IsTrue(server.Players[0].Planets.Count > 0);
            Assert.IsTrue(server.Players[0].Fleets.Count > 0);
        }

    }

}