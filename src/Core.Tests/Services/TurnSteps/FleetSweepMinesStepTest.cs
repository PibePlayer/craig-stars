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
    public class FleetSweepMinesStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetSweepMinesStepTest));

        FleetService fleetService = TestUtils.TestContainer.GetInstance<FleetService>();

        [Test]
        public void ProcessTest()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var fleet = game.Fleets[0];

            var mineField = new MineField()
            {
                PlayerNum = player2.Num,
                NumMines = 1000,
            };
            game.MineFields.Add(mineField);

            // should ignore this mineField
            var playerMineField = new MineField()
            {
                PlayerNum = player1.Num,
                NumMines = 1000
            };
            game.MineFields.Add(playerMineField);

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player1, new ShipDesign()
            {
                PlayerNum = player1.Num,
                Name = "Mine Sweeping Scout",
                Hull = Techs.Scout,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.MiniGun, 3, 1),
                }
            });
            gameRunner.ComputeSpecs(recompute: true);

            FleetSweepMinesStep step = new FleetSweepMinesStep(gameRunner.GameProvider, fleetService);

            // minigun sweeps 256 enemy mines
            step.Process();
            Assert.AreEqual(2, game.MineFields.Count);
            Assert.AreEqual(1000 - fleet.Spec.MineSweep, game.MineFields[0].NumMines);

            // it should ignore our mineField
            Assert.AreEqual(1000, game.MineFields[1].NumMines);

            // make sure it destroys a minefield that is low
            mineField.NumMines = 250;
            step.Process();
            gameRunner.OnPurgeDeletedMapObjects();
            Assert.AreEqual(1, game.MineFields.Count);
            Assert.AreEqual(1000, game.MineFields[0].NumMines);
            Assert.AreEqual(fleet.PlayerNum, game.MineFields[0].PlayerNum);

        }
    }
}