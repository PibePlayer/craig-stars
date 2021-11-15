using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetAggregatorTest
    {
        FleetAggregator fleetAggregator;

        [SetUp]
        public void SetUp()
        {
            IRulesProvider rulesProvider = TestUtils.TestContainer.GetInstance<IRulesProvider>();
            PlayerService playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
            fleetAggregator = new FleetAggregator(rulesProvider, playerService);
        }

        [Test]
        public void TestComputeStarbaseAggregate()
        {
            // make a starbase with two mass drivers
            var player = new Player();
            var design = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Death Star",
                Hull = Techs.DeathStar,
                HullSetNumber = 0,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.MassDriver7, 1, 1),
                    new ShipDesignSlot(Techs.MassDriver7, 11, 1),
                }
            };

            var starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                }
            };

            fleetAggregator.ComputeStarbaseAggregate(player, starbase);

            Assert.AreEqual(7, starbase.Aggregate.BasePacketSpeed);
            Assert.AreEqual(8, starbase.Aggregate.SafePacketSpeed);
        }

        [Test]
        public void TestComputeCloak()
        {
            var design = ShipDesigns.LongRangeScount.Clone();
            var player = new Player();
            design.PlayerNum = player.Num;

            var fleet = new Fleet()
            {
                PlayerNum = design.PlayerNum,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                }
            };

            fleetAggregator.ComputeDesignAggregate(player, design, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);

            Assert.AreEqual(0, fleet.Aggregate.CloakPercent);

            design.Slots[2].HullComponent = Techs.StealthCloak;
            fleetAggregator.ComputeDesignAggregate(player, design, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

            // should be the same for 2 identical tokens
            fleet.Tokens[0].Quantity = 2;
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestComputeCloakMultiShip()
        {
            var player = new Player();
            var scoutDesign = ShipDesigns.LongRangeScount.Clone();
            scoutDesign.PlayerNum = player.Num;

            var cloakedScoutDesign = new ShipDesign()
            {
                PlayerNum = player.Num,
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
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(scoutDesign, 1),
                  new ShipToken(cloakedScoutDesign, 1),
                }
            };

            // cloak goes down because extra ship counts as cargo
            fleetAggregator.ComputeDesignAggregate(player, scoutDesign, recompute: true);
            fleetAggregator.ComputeDesignAggregate(player, cloakedScoutDesign, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(23, fleet.Aggregate.CloakPercent);
        }

        [Test]
        public void TestComputeCloakCargo()
        {
            var player = new Player();

            var freighterDesign = new ShipDesign()
            {
                PlayerNum = player.Num,
                Hull = Techs.SmallFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.Tritanium, 2, 1),
                    new ShipDesignSlot(Techs.StealthCloak, 3, 1)
                }
            };

            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(freighterDesign, 1)
                }
            };

            fleetAggregator.ComputeDesignAggregate(player, freighterDesign, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Aggregate.CloakPercent);

            // fill it with cargo
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            fleetAggregator.ComputeDesignAggregate(player, freighterDesign, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(20, fleet.Aggregate.CloakPercent);

        }

        [Test]
        public void TestComputeCloakSuperStealth()
        {
            var player = new Player();
            player.Race.PRT = PRT.SS;

            var freighterDesign = new ShipDesign()
            {
                PlayerNum = player.Num,
                Hull = Techs.SmallFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                    new ShipDesignSlot(Techs.Tritanium, 2, 1),
                }
            };

            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(freighterDesign, 1)
                }
            };

            fleetAggregator.ComputeDesignAggregate(player, freighterDesign, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
            Assert.AreEqual(75, fleet.Aggregate.CloakPercent);

            // fill it with cargo, should be the same cloak
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            fleetAggregator.ComputeDesignAggregate(player, freighterDesign, recompute: true);
            fleetAggregator.ComputeAggregate(player, fleet, recompute: true);
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
            fleetAggregator.ComputeFleetComposition(fleet);
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
            fleetAggregator.ComputeFleetComposition(fleet);
            Assert.IsFalse(fleet.Aggregate.FleetCompositionComplete);
            Assert.AreEqual(2, fleet.Aggregate.FleetCompositionTokensRequired.Count);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Aggregate.FleetCompositionTokensRequired[0].Purpose);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Aggregate.FleetCompositionTokensRequired[0].Purpose);

        }
    }

}