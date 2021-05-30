using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetTest
    {
        Rules rules = new Rules(0);

        /// <summary>
        /// Helper method to get a long range scout fleet
        /// </summary>
        /// <returns></returns>
        Fleet GetLongRangeScout()
        {
            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = new Player();

            var fleet = new Fleet()
            {
                Player = design.Player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                }
            };

            fleet.ComputeAggregate();
            fleet.Fuel = fleet.FuelCapacity;
            return fleet;
        }

        [Test]
        public void TestComputeCloak()
        {
            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = new Player();

            var fleet = new Fleet()
            {
                Player = design.Player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                }
            };

            fleet.ComputeAggregate();

            Assert.AreEqual(0, fleet.Aggregate.CloakPercent);

            design.Slots[2].HullComponent = Techs.StealthCloak;
            design.ComputeAggregate(recompute: true);
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

            // should be the same for 2 identical tokens
            fleet.Tokens[0].Quantity = 2;
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestComputeCloakMultiShip()
        {
            var player = new Player();
            var scoutDesign = ShipDesigns.LongRangeScount.Clone();
            scoutDesign.Player = player;

            var cloakedScoutDesign = new ShipDesign()
            {
                Player = player,
                Hull = Techs.Scout,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.LongHump6, 1, 1),
                    new ShipDesignSlot(Techs.FuelTank, 2, 1),
                    new ShipDesignSlot(Techs.StealthCloak, 3, 1),
                }
            };

            var fleet = new Fleet()
            {
                Player = player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(scoutDesign, 1),
                  new ShipToken(cloakedScoutDesign, 1),
                }
            };

            // cloak goes down because extra ship counts as cargo
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(23, fleet.Aggregate.CloakPercent);
        }

        [Test]
        public void TestComputeCloakCargo()
        {
            var player = new Player();

            var freighterDesign = new ShipDesign()
            {
                Player = player,
                Hull = Techs.SmallFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.Tritanium, 2, 1),
                    new ShipDesignSlot(Techs.StealthCloak, 3, 1)
                }
            };

            var fleet = new Fleet()
            {
                Player = player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(freighterDesign, 1)
                }
            };

            fleet.ComputeAggregate();
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

            // fill it with cargo
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            freighterDesign.ComputeAggregate(recompute: true);
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(20, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestComputeCloakSuperStealth()
        {
            var player = new Player();
            player.Race.PRT = PRT.SS;

            var freighterDesign = new ShipDesign()
            {
                Player = player,
                Hull = Techs.SmallFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.Tritanium, 2, 1),
                }
            };

            var fleet = new Fleet()
            {
                Player = player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(freighterDesign, 1)
                }
            };

            fleet.ComputeAggregate();
            Assert.AreEqual(75, fleet.Aggregate.CloakPercent);

            // fill it with cargo, should be the same cloak
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            freighterDesign.ComputeAggregate(recompute: true);
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(75, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestSplit()
        {
            var player = new Player();
            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = player;

            var fleet = new Fleet()
            {
                Id = 1,
                BaseName = "Long Range Scout",
                Name = "Long Range Scout #1",
                Player = player,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 3)
                }
            };
            player.Fleets.Add(fleet);

            var splitFleetOrder = new SplitAllFleetOrder();
            var splitFleets = fleet.Split(splitFleetOrder);

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
            var fleet = GetLongRangeScout();

            var wp0 = Waypoint.PositionWaypoint(new Vector2(0, 0));
            var wp1 = Waypoint.PositionWaypoint(new Vector2(49, 0));

            // we should be able to make this at warp 7
            Assert.AreEqual(7, fleet.GetBestWarpFactor(wp0, wp1));

            // max out our warp if we are far away
            wp1 = Waypoint.PositionWaypoint(new Vector2(100, 0));
            Assert.AreEqual(9, fleet.GetBestWarpFactor(wp0, wp1));

            // ensure we make it if we are REALLY far away
            wp1 = Waypoint.PositionWaypoint(new Vector2(500, 0));
            Assert.AreEqual(8, fleet.GetBestWarpFactor(wp0, wp1));
        }

    }

}