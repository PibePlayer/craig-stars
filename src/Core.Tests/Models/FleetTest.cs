using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetTest
    {
        Rules rules = new Rules(0);

        [Test]
        public void TestComputeCloak()
        {
            var design = ShipDesigns.LongRangeScount.Clone();
            var player = new Player();
            design.Player = player;

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
            design.ComputeAggregate(player, recompute: true);
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

            freighterDesign.ComputeAggregate(player, recompute: true);
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

            freighterDesign.ComputeAggregate(player, recompute: true);
            fleet.ComputeAggregate(recompute: true);
            Assert.AreEqual(75, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestComputeFleetComposition()
        {
            // make a simple bomber fleet
            Fleet fleet = new()
            {
                Tokens = new()
                {
                    new ShipToken(ShipDesigns.MiniBomber, 1)
                }
            };

            // empty fleet is already complete
            fleet.ComputeFleetComposition();
            Assert.IsTrue(fleet.Aggregate.FleetCompositionComplete);
            Assert.AreEqual(0, fleet.Aggregate.FleetCompositionTokensRequired.Count);

            // make a new fleet composition that requires 2 bombers and a fuel frieghter
            FleetComposition fleetComposition = new()
            {
                Type = FleetCompositionType.Bomber,
                Tokens = new()
                {
                    new FleetCompositionToken(ShipDesignPurpose.Bomber, 2),
                    new FleetCompositionToken(ShipDesignPurpose.FuelFreighter, 1),
                }
            };

            fleet.FleetComposition = fleetComposition;
            fleet.ComputeFleetComposition();
            Assert.IsFalse(fleet.Aggregate.FleetCompositionComplete);
            Assert.AreEqual(2, fleet.Aggregate.FleetCompositionTokensRequired.Count);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Aggregate.FleetCompositionTokensRequired[0].Purpose);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Aggregate.FleetCompositionTokensRequired[0].Purpose);

        }


    }

}