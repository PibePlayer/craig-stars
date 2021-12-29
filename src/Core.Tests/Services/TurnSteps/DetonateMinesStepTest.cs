using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

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
            game.AddMapObject(mineField);
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
            game.MoveMapObject(game.Fleets[0], game.Fleets[0].Position, new Vector2(100, 100)); // out of the blast radius
            var player = game.Players[0];
            var mineField = new MineField()
            {
                PlayerNum = player.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
            };
            game.AddMapObject(mineField);
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
            game.AddMapObject(mineField);

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = TestUtils.CreateDesign(game, player, ShipDesigns.LittleHen.Clone(player));
            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
            };
            game.AddMapObject(fleet);
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