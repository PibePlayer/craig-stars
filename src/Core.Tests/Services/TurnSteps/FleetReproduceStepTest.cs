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
    public class FleetReproduceStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetReproduceStepTest));

        [Test]
        public void FleetReproduceTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var player = game.Players[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player,
                new ShipDesign()
                {
                    PlayerNum = fleet.PlayerNum,
                    Name = "Medium Freighter",
                    Hull = Techs.MediumFreighter,
                    Slots = new List<ShipDesignSlot>() {
                        new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    }
                }
            );

            player.Race.PRT = PRT.IS;
            player.Race.GrowthRate = 10;
            fleet.Cargo = fleet.Cargo.WithColonists(100);

            gameRunner.ComputeSpecs(recompute: true);

            FleetReproduceStep step = new FleetReproduceStep(gameRunner.GameProvider);

            step.Reproduce(fleet, player);
            Assert.AreEqual(105, fleet.Cargo.Colonists);

        }

        [Test]
        public void FleetReproduceOverTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var player = game.Players[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player, new ShipDesign()
            {
                PlayerNum = fleet.PlayerNum,
                Name = "Medium Freighter",
                Hull = Techs.MediumFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                }
            });
            player.Race.PRT = PRT.IS;
            player.Race.GrowthRate = 10;

            gameRunner.ComputeSpecs(recompute: true);

            // leave space for 1kT of colonists
            fleet.Cargo = fleet.Cargo.WithColonists(fleet.Spec.CargoCapacity - 1);

            FleetReproduceStep step = new FleetReproduceStep(gameRunner.GameProvider);

            // should fill cargo, but no more
            step.Reproduce(fleet, player);
            Assert.AreEqual(fleet.Spec.CargoCapacity, fleet.Cargo.Colonists);

            fleet.Orbiting = game.Planets[0];
            var planetStartingPop = game.Planets[0].Population;

            // should overflow to planet
            step.Reproduce(fleet, player);
            Assert.AreEqual(fleet.Spec.CargoCapacity, fleet.Cargo.Colonists);
            Assert.AreEqual(planetStartingPop + Utils.Utils.RoundToNearest((int)(21 * 100 * .5)), game.Planets[0].Population);

        }

        [Test]
        public void StepTest()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];
            var player = game.Players[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = TestUtils.CreateDesign(game, player, new ShipDesign()
            {
                PlayerNum = fleet.PlayerNum,
                Name = "Medium Freighter",
                Hull = Techs.MediumFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                }
            });
            player.Race.PRT = PRT.IS;
            player.Race.GrowthRate = 10;
            fleet.Cargo = fleet.Cargo.WithColonists(100);

            gameRunner.ComputeSpecs(recompute: true);

            FleetReproduceStep step = new FleetReproduceStep(gameRunner.GameProvider);

            step.Process();
            Assert.AreEqual(105, fleet.Cargo.Colonists);

            player.Race.PRT = PRT.JoaT;
            gameRunner.ComputeSpecs(recompute: true);

            // shouldn't grow at all
            step.Process();
            Assert.AreEqual(105, fleet.Cargo.Colonists);
        }
    }
}