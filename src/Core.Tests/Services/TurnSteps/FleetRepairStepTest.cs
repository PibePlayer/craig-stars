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
    public class FleetRepairStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetRepairStepTest));

        Game game;
        GameRunner gameRunner;
        FleetRepairStep step;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new FleetRepairStep(gameRunner.GameProvider, new TestRulesProvider());
        }

        [Test]
        public void TestRepairFleet()
        {
            var player = game.Players[0];
            var fleet = game.Fleets[0];
            var planet = game.Planets[0];

            // damage us by 10%
            fleet.Orbiting = null;
            fleet.Aggregate.Armor = 100;
            fleet.Damage = 10;

            // should repair 2% of our damage because we're stopped
            step.RepairFleet(fleet, player, null);
            Assert.AreEqual(8, fleet.Damage);

            // orbiting this starbase is better
            // should repair 20% of our armor because the planet has a starbase with a stardoc
            fleet.Damage = 25;
            step.RepairFleet(fleet, player, planet);
            Assert.AreEqual(5, fleet.Damage);

            // test orbiting 
            fleet.Damage = 10;
            planet.PlayerNum = MapObject.Unowned;
            step.RepairFleet(fleet, player, planet);
            Assert.AreEqual(7, fleet.Damage);

            // test moving
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 100)));

            // should repair 1% of our damage because we're moving
            fleet.Damage = 10;
            step.RepairFleet(fleet, player, null);
            Assert.AreEqual(9, fleet.Damage);

        }

        [Test]
        public void TestRepairFleetBombing()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();

            var bombingFleet = game.Fleets[0];
            var bombingPlayer = game.Players[0];
            var defendingPalnet = game.Planets[1];

            bombingFleet.Tokens.Add(new ShipToken()
            {
                Design = new ShipDesign()
                {
                    PlayerNum = bombingPlayer.Num,
                    Hull = Techs.MiniBomber,
                    Slots = new List<ShipDesignSlot>() {
                                new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                                new ShipDesignSlot(Techs.LadyFingerBomb, 2, 2),
                            },
                },
                Quantity = 1,
            });
            game.Designs.Add(bombingFleet.Tokens.Last().Design);

            gameRunner.ComputeAggregates(recompute: true);

            bombingFleet.Orbiting = defendingPalnet;
            defendingPalnet.OrbitingFleets.Add(bombingFleet);

            bombingFleet.Aggregate.Armor = 100;
            bombingFleet.Damage = 10;
            step.RepairFleet(bombingFleet, bombingPlayer, defendingPalnet);

            // shouldn't repair at all while bombing
            Assert.AreEqual(10, bombingFleet.Damage);

        }

        [Test]
        public void TestRepairStarbase()
        {
            var player = game.Players[0];
            var starbase = game.Planets[0].Starbase;

            // damage us by 20%
            starbase.Aggregate.Armor = 100;
            starbase.Damage = 20;

            // should repair 2% of our damage because we're stopped
            step.RepairStarbase(starbase, player);
            Assert.AreEqual(10, starbase.Damage);
        }

        [Test]
        public void TestRepairIS()
        {
            var player = game.Players[0];
            player.Race.PRT = PRT.IS;
            gameRunner.ComputeAggregates();

            var fleet = game.Fleets[0];
            var planet = game.Planets[0];

            // damage us by 10%
            fleet.Orbiting = null;
            fleet.Aggregate.Armor = 100;
            fleet.Damage = 10;

            // should repair 4% of our damage because we're stopped and we're IS
            step.RepairFleet(fleet, player, null);
            Assert.AreEqual(6, fleet.Damage);

            // test repairing a starbase as IS
            // damage us by 20%
            var starbase = game.Planets[0].Starbase;
            starbase.Aggregate.Armor = 100;
            starbase.Damage = 20;

            // should repair 2% of our damage because we're stopped
            step.RepairStarbase(starbase, player);
            Assert.AreEqual(5, starbase.Damage);

        }
    }
}