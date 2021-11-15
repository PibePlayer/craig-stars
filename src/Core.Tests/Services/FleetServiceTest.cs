using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetServiceTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetServiceTest));

        Rules rules = new Rules(0);
        FleetAggregator fleetAggregator = TestUtils.TestContainer.GetInstance<FleetAggregator>();
        FleetService service;

        [SetUp]
        public void SetUp()
        {
            service = new FleetService(fleetAggregator);
        }

        [Test]
        public void TestSplit()
        {
            var player = new Player();
            var design = ShipDesigns.LongRangeScount.Clone();
            design.PlayerNum = player.Num;

            var fleet = new Fleet()
            {
                Id = 1,
                BaseName = "Long Range Scout",
                Name = "Long Range Scout #1",
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
        public void TestGetBestWarpFactor()
        {
            var player = new Player();
            var fleet = TestUtils.GetLongRangeScout(player);

            var wp0 = Waypoint.PositionWaypoint(new Vector2(0, 0));
            var wp1 = Waypoint.PositionWaypoint(new Vector2(49, 0));

            // we should be able to make this at warp 7
            Assert.AreEqual(7, service.GetBestWarpFactor(fleet, player, wp0, wp1));

            // max out our warp if we are far away
            wp1 = Waypoint.PositionWaypoint(new Vector2(100, 0));
            Assert.AreEqual(9, service.GetBestWarpFactor(fleet, player, wp0, wp1));

            // ensure we make it if we are REALLY far away
            wp1 = Waypoint.PositionWaypoint(new Vector2(500, 0));
            Assert.AreEqual(8, service.GetBestWarpFactor(fleet, player, wp0, wp1));
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
    }
}
