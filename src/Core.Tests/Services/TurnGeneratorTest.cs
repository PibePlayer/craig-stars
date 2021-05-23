using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TurnGeneratorTest
    {

        [Test]
        public void TestGenerateTurn()
        {
            var game = GameTest.GetSingleUnitGame();
            var tg = new TurnGenerator(game);
            tg.GenerateTurn();
            // game.RunTurnProcessors();

            // make sure our planet grew pop
            Assert.IsTrue(game.Planets[0].Population > 25000);
        }

    }

}