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
    public class FleetLayMinesStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetLayMinesStepTest));

        [Test]
        public void LayMineFieldTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player, new ShipDesign()
            {
                PlayerNum = fleet.PlayerNum,
                Name = "Mine Laying Scout",
                Hull = Techs.Scout,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.MineDispenser50, 3, 1),
                }
            });

            gameRunner.ComputeSpecs(recompute: true);

            FleetLayMinesStep step = new FleetLayMinesStep(gameRunner.GameProvider);

            step.LayMineField(fleet, player);
            Assert.AreEqual(1, game.MineFields.Count);
            Assert.AreEqual(50, game.MineFields[0].NumMines);
            Assert.AreEqual(MineFieldType.Standard, game.MineFields[0].Type);

            // lay a second time and it should add to the existing MineField
            step.LayMineField(fleet, player);
            Assert.AreEqual(1, game.MineFields.Count);
            Assert.AreEqual(100, game.MineFields[0].NumMines);
            Assert.AreEqual(MineFieldType.Standard, game.MineFields[0].Type);

        }

        [Test]
        public void LayMineFieldMovingTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player, new ShipDesign()
            {
                PlayerNum = fleet.PlayerNum,
                Name = "Mine Laying Scout",
                Hull = Techs.Scout,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.MineDispenser50, 3, 1),
                }
            });
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 100)));

            gameRunner.ComputeSpecs(recompute: true);

            FleetLayMinesStep step = new FleetLayMinesStep(gameRunner.GameProvider);

            step.LayMineField(fleet, player);
            Assert.AreEqual(0, game.MineFields.Count);

            // SD races can lay mines while moving
            player.Race.PRT = PRT.SD;
            gameRunner.ComputeSpecs();

            step.LayMineField(fleet, player);
            Assert.AreEqual(1, game.MineFields.Count);
            Assert.AreEqual(25, game.MineFields[0].NumMines);
            Assert.AreEqual(MineFieldType.Standard, game.MineFields[0].Type);
        }
    }
}