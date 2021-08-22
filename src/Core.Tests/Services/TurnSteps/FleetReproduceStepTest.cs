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
            var game = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = new ShipDesign()
            {
                Player = fleet.Player,
                Name = "Medium Freighter",
                Hull = Techs.MediumFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                }
            };
            fleet.Player.Race.PRT = PRT.IS;
            fleet.Player.Race.GrowthRate = 10;
            fleet.Cargo = fleet.Cargo.WithColonists(100);

            fleet.ComputeAggregate();

            FleetReproduceStep step = new FleetReproduceStep(game);

            step.Reproduce(fleet);
            Assert.AreEqual(105, fleet.Cargo.Colonists);

        }

        [Test]
        public void FleetReproduceOverTest()
        {
            var game = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = new ShipDesign()
            {
                Player = fleet.Player,
                Name = "Medium Freighter",
                Hull = Techs.MediumFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                }
            };
            fleet.Player.Race.PRT = PRT.IS;
            fleet.Player.Race.GrowthRate = 10;

            fleet.ComputeAggregate();

            // leave space for 1kT of colonists
            fleet.Cargo = fleet.Cargo.WithColonists(fleet.Aggregate.CargoCapacity - 1);

            FleetReproduceStep step = new FleetReproduceStep(game);

            // should fill cargo, but no more
            step.Reproduce(fleet);
            Assert.AreEqual(fleet.Aggregate.CargoCapacity, fleet.Cargo.Colonists);

            fleet.Orbiting = game.Planets[0];
            var planetStartingPop = game.Planets[0].Population;
            
            // should overflow to planet
            step.Reproduce(fleet);
            Assert.AreEqual(fleet.Aggregate.CargoCapacity, fleet.Cargo.Colonists);
            Assert.AreEqual(planetStartingPop + Utils.Utils.RoundToNearest((int)(21 * 100 * .5)), game.Planets[0].Population);

        }

        [Test]
        public void StepTest()
        {
            var game = TestUtils.GetSingleUnitGame();
            var fleet = game.Fleets[0];

            // make the fleet have a simple scout design with a mine dispenser 50
            fleet.Tokens[0].Design = new ShipDesign()
            {
                Player = fleet.Player,
                Name = "Medium Freighter",
                Hull = Techs.MediumFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                }
            };
            fleet.Player.Race.PRT = PRT.IS;
            fleet.Player.Race.GrowthRate = 10;
            fleet.Cargo = fleet.Cargo.WithColonists(100);

            fleet.ComputeAggregate();

            FleetReproduceStep step = new FleetReproduceStep(game);

            step.Process();
            Assert.AreEqual(105, fleet.Cargo.Colonists);

            fleet.Player.Race.PRT = PRT.JoaT;

            // shouldn't grow at all
            step.Process();
            Assert.AreEqual(105, fleet.Cargo.Colonists);
        }
    }
}