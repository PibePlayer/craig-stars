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
    public class DecayMinesStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStepTest));

        [Test]
        public void DecayTest()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player1 = game.Players[0];
            var mineField = new MineField()
            {
                PlayerNum = player1.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(100, 100), // move it away from the planet
            };
            game.MineFields.Add(mineField);

            DecayMinesStep step = new DecayMinesStep(game);

            // regular decay is 2%
            mineField.NumMines = 1000;
            step.Decay(mineField);
            Assert.AreEqual(980, mineField.NumMines);
            Assert.AreEqual(1, game.MineFields.Count);

            // min decay is 10
            mineField.NumMines = 100;
            step.Decay(mineField);
            Assert.AreEqual(90, mineField.NumMines);
            Assert.AreEqual(1, game.MineFields.Count);

            // decay rate is -4% per planet
            mineField.NumMines = 1000;
            mineField.Position = new Vector2(0, 0);
            step.Decay(mineField);
            Assert.AreEqual(940, mineField.NumMines);
            Assert.AreEqual(1, game.MineFields.Count);

            // minefield goes away
            mineField.NumMines = 20;
            step.Decay(mineField);
            game.PurgeDeletedMapObjects();
            Assert.AreEqual(10, mineField.NumMines);
            Assert.AreEqual(0, game.MineFields.Count);

        }
    }
}