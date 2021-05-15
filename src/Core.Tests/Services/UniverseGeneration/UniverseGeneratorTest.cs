using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using CraigStars.UniverseGeneration;

namespace CraigStars.Tests
{
    [TestFixture]
    public class UniverseGeneratorTest
    {

        [Test]
        public void TestGenerate()
        {
            var game = new Game() { SaveToDisk = false };
            game.Init(new List<Player>() { new Player() { AIControlled = true } }, new Rules(0), StaticTechStore.Instance, new TestGamesManager(), new TestTurnProcessorManager());
            game.Rules.StartMode = GameStartMode.Normal;

            var ug = new UniverseGenerator(game);
            ug.Generate();

            Assert.AreEqual(game.Rules.NumPlanets, game.Planets.Count);
            Assert.Greater(game.Fleets.Count, 0);
            var player = game.Players[0];
            Assert.NotNull(game.Players[0].Homeworld);
            Assert.AreEqual(game.Players[0].TechLevels, new TechLevel(3, 3, 3, 3, 3, 3));
        }

    }

}