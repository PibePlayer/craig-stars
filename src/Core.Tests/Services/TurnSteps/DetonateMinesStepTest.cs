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
            gameRunner.ComputeSpecs();

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
            var player = game.Players[0];
            var mineField = new MineField()
            {
                PlayerNum = player.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
            };
            game.MineFields.Add(mineField);
            gameRunner.ComputeSpecs();

            DetonateMinesStep step = new DetonateMinesStep(gameRunner.GameProvider, mineFieldDamager);

            Assert.AreEqual(1, game.Fleets.Count);

            // detonate
            mineField.NumMines = 1000;
            step.Detonate(mineField);

            // bye bye scout
            Assert.AreEqual(1, game.Fleets.Count);
            Assert.AreEqual(0, game.Fleets[0].Tokens[0].Damage);

        }

        [Test]
        public void TestTakeMineFieldDetonateDamage()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            player.Race.PRT = PRT.SD;

            var mineField = new MineField()
            {
                PlayerNum = player.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 320,
                Detonate = true,
            };
            game.MineFields.Add(mineField);

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = TestUtils.CreateDesign(game, player, ShipDesigns.LittleHen.Clone(player));
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
            };
            game.Fleets.Add(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            Assert.AreEqual(2, game.Fleets.Count);

            DetonateMinesStep step = new DetonateMinesStep(gameRunner.GameProvider, mineFieldDamager);
            step.Detonate(mineField);
            gameRunner.OnPurgeDeletedMapObjects();

            // one fleet should die
            Assert.AreEqual(1, game.Fleets.Count);
            Assert.AreEqual(fleet, game.Fleets[0]);
            Assert.AreEqual(0, game.Fleets[0].Damage);
        }
    }
}