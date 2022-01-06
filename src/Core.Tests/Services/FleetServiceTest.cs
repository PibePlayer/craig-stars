using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetServiceTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetServiceTest));

        Rules rules = new Rules(0);
        FleetSpecService fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        FleetService service;

        [SetUp]
        public void SetUp()
        {
            service = new FleetService(fleetSpecService);
        }

        [Test]
        public void TestSplit()
        {
            var player = new Player() { Num = 0 };
            var design = ShipDesigns.LongRangeScount.Clone(player);

            var fleet = new Fleet()
            {
                Id = 1,
                BaseName = "Long Range Scout",
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 3)
                }
            };
            player.Fleets.Add(fleet);

            var splitFleetOrder = new SplitAllFleetOrder();
            var splitFleets = service.Split(fleet, player, splitFleetOrder);

            // we should get two more fleets with incremented ids
            Assert.AreEqual(2, splitFleets.Count);
            Assert.AreEqual("Long Range Scout #2", splitFleets[0].Name);
            Assert.AreEqual("Long Range Scout #3", splitFleets[1].Name);
            Assert.AreEqual(2, splitFleets[0].Id);
            Assert.AreEqual(3, splitFleets[1].Id);

            Assert.AreEqual(1, splitFleets[0].Tokens.Count);
            Assert.AreEqual(1, splitFleets[1].Tokens.Count);
            Assert.AreEqual(design, splitFleets[0].Tokens[0].Design);
            Assert.AreEqual(design, splitFleets[1].Tokens[0].Design);
        }

        [Test]
        public void TestMerge()
        {
            var player = new Player() { Num = 0 };
            var design1 = ShipDesigns.LongRangeScount.Clone(player);
            var design2 = ShipDesigns.SantaMaria.Clone(player);

            var fleet1 = new Fleet()
            {
                Id = 1,
                BaseName = "Long Range Scout",
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design1, 1)
                }
            };

            var fleet2 = new Fleet()
            {
                Id = 2,
                BaseName = "Scout and Colonizer",
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design1, 1),
                  new ShipToken(design2, 1)
                }
            };

            var order = new MergeFleetOrder(fleet1.Guid, fleet2);
            service.Merge(fleet1, player, new List<Fleet>() { fleet2 });

            // we should get two more fleets with incremented ids
            Assert.AreEqual(2, fleet1.Tokens.Count);
            Assert.AreEqual("Long Range Scout #1", fleet1.Name);

            Assert.AreEqual(design1, fleet1.Tokens[0].Design);
            Assert.AreEqual(design2, fleet1.Tokens[1].Design);
        }

        [Test]
        public void TestGetDefaultWarpFactor()
        {
            var player = new Player();
            var fleet = TestUtils.GetLongRangeScout(player);

            // should be 6 with the longhump 6
            Assert.AreEqual(6, service.GetDefaultWarpFactor(fleet, player));

            fleet.Tokens[0].Design.Slots[0].HullComponent = Techs.DaddyLongLegs7;

            // should use 7, the ideal engine speed of the Daddy Long Legs 7
            fleetSpecService.ComputeDesignSpec(player, fleet.Tokens[0].Design, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(7, service.GetDefaultWarpFactor(fleet, player));
        }

        [Test]
        public void TestGetBestWarpFactor()
        {
            var player = new Player();
            var fleet = TestUtils.GetLongRangeScout(player);

            var wp0 = Waypoint.PositionWaypoint(new Vector2(0, 0));
            var wp1 = Waypoint.PositionWaypoint(new Vector2(49, 0));

            // we should be able to make this at warp 7
            Assert.AreEqual(7, service.GetBestWarpFactor(fleet, player, wp0, wp1));

            // don't go above 2x fuel usage
            wp1 = Waypoint.PositionWaypoint(new Vector2(100, 0));
            Assert.AreEqual(7, service.GetBestWarpFactor(fleet, player, wp0, wp1));

            // ensure we make it if we are REALLY far away
            wp1 = Waypoint.PositionWaypoint(new Vector2(500, 0));
            Assert.AreEqual(7, service.GetBestWarpFactor(fleet, player, wp0, wp1));
        }

        [Test]
        public void TestWillAttack()
        {
            var attacker = new Player() { Num = 0 };
            var defender = new Player() { Num = 1 };
            var fleet = TestUtils.GetStalwartDefender(attacker);
            var fleetScout = TestUtils.GetLongRangeScout(attacker);

            // won't attack self
            Assert.AreEqual(false, service.WillAttack(fleet, attacker, attacker.Num));

            // will attack other player
            Assert.AreEqual(true, service.WillAttack(fleet, attacker, defender.Num));

            // won't attack without weapons
            Assert.AreEqual(false, service.WillAttack(fleetScout, attacker, defender.Num));

        }

        [Test]
        public void TestGetBestFuelUsage()
        {
            var player = new Player();
            var fleet = TestUtils.GetFleet(player, ShipDesigns.Teamster);

            fleet.Tokens[0].Design.Slots[0].HullComponent = Techs.QuickJump5;
            fleet.Tokens[0].Design.Spec.Engine = Techs.QuickJump5;

            // warp 0 and 1 costs nothing
            Assert.AreEqual(0, service.GetFuelCost(fleet, player, warpFactor: 0, distance: 0));
            Assert.AreEqual(0, service.GetFuelCost(fleet, player, warpFactor: 1, distance: 100));

            // use some normal fuel at warp 5
            int fuelUsed = 17; 
            Assert.AreEqual(fuelUsed, service.GetFuelCost(fleet, player, warpFactor: 5, distance: 25));

            // test the fuel mizer
            // it should use no fuel at warp 4, a very small amount at warp 5, and 120% at warp 6
            fleet.Tokens[0].Design.Slots[0].HullComponent = Techs.FuelMizer;
            fleet.Tokens[0].Design.Spec.Engine = Techs.FuelMizer;
            Assert.AreEqual(0, service.GetFuelCost(fleet, player, warpFactor: 4, distance: 25));
            Assert.AreEqual((int)(fuelUsed * .35 + .5), service.GetFuelCost(fleet, player, warpFactor: 5, distance: 25));
            Assert.AreEqual((int)(fuelUsed * 1.2 + .5), service.GetFuelCost(fleet, player, warpFactor: 6, distance: 25));


        }
    }
}
