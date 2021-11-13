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
    public class DetonateMinesStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(DetonateMinesStepTest));

        MineFieldDamager mineFieldDamager = new();

        [Test]
        public void DetonateTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player1 = game.Players[0];
            var mineField = new MineField()
            {
                PlayerNum = player1.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
            };
            game.MineFields.Add(mineField);
            game.ComputeAggregates();

            DetonateMinesStep step = new DetonateMinesStep(gameRunner.GameProvider, mineFieldDamager);

            Assert.AreEqual(1, game.Fleets.Count);

            // detonate
            mineField.NumMines = 1000;
            step.Detonate(mineField);

            // bye bye scout
            gameRunner.OnPurgeDeletedMapObjects();
            Assert.AreEqual(0, game.Fleets.Count);

        }

        [Test]
        public void DetonateSafeFleetTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            game.Fleets[0].Position = new Vector2(100, 100); // out of the blast radius
            var player1 = game.Players[0];
            var mineField = new MineField()
            {
                PlayerNum = player1.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
            };
            game.MineFields.Add(mineField);
            game.ComputeAggregates();

            DetonateMinesStep step = new DetonateMinesStep(gameRunner.GameProvider, mineFieldDamager);

            Assert.AreEqual(1, game.Fleets.Count);

            // detonate
            mineField.NumMines = 1000;
            step.Detonate(mineField);

            // bye bye scout
            Assert.AreEqual(1, game.Fleets.Count);
            Assert.AreEqual(0, game.Fleets[0].Tokens[0].Damage);

        }
    }
}