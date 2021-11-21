using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetSpecServiceTest
    {
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            IRulesProvider rulesProvider = TestUtils.TestContainer.GetInstance<IRulesProvider>();
            fleetSpecService = new FleetSpecService(rulesProvider);
        }

        [Test]
        public void TestComputeStarbaseSpec()
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
            fleetSpecService.ComputeDesignSpec(player, design);

            var starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                },
            };

            fleetSpecService.ComputeStarbaseSpec(player, starbase);

            Assert.AreEqual(7, starbase.Spec.BasePacketSpeed);
            Assert.AreEqual(8, starbase.Spec.SafePacketSpeed);

            player.Race.Spec.StarbaseBuiltInCloakUnits = 40;
            fleetSpecService.ComputeStarbaseSpec(player, starbase, recompute: true);
            Assert.AreEqual(20, starbase.Spec.CloakPercent);
        }

        [Test]
        public void TestComputeStarbaseCost()
        {
            // make a starbase with two mass drivers
            var player = new Player();
            var design = ShipDesigns.Starbase.Clone(player);

            fleetSpecService.ComputeDesignCost(player, design);

            Assert.AreEqual(new Cost(136, 176, 266, 744), design.Spec.Cost);

            player.Race.Spec.StarbaseCostFactor = .8f;
            fleetSpecService.ComputeDesignCost(player, design);
            // Real stars has 596 resources for this starbase and an AR player
            Assert.AreEqual(new Cost(109, 141, 213, 595), design.Spec.Cost);
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

            fleetSpecService.ComputeDesignSpec(player, design, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);

            Assert.AreEqual(0, fleet.Spec.CloakPercent);

            design.Slots[2].HullComponent = Techs.StealthCloak;
            fleetSpecService.ComputeDesignSpec(player, design, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Spec.CloakPercent);

            // should be the same for 2 identical tokens
            fleet.Tokens[0].Quantity = 2;
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Spec.CloakPercent);

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
            fleetSpecService.ComputeDesignSpec(player, scoutDesign, recompute: true);
            fleetSpecService.ComputeDesignSpec(player, cloakedScoutDesign, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(23, fleet.Spec.CloakPercent);
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

            fleetSpecService.ComputeDesignSpec(player, freighterDesign, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(35, fleet.Spec.CloakPercent);

            // fill it with cargo
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            fleetSpecService.ComputeDesignSpec(player, freighterDesign, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(20, fleet.Spec.CloakPercent);

        }

        [Test]
        public void TestComputeCloakSuperStealth()
        {
            var player = new Player();
            player.Race.PRT = PRT.SS;
            var raceService = TestUtils.TestContainer.GetInstance<RaceService>();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);

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

            fleetSpecService.ComputeDesignSpec(player, freighterDesign, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(75, fleet.Spec.CloakPercent);

            // fill it with cargo, should be the same cloak
            fleet.Cargo = Cargo.OfAmount(CargoType.Ironium, fleet.AvailableCapacity);

            fleetSpecService.ComputeDesignSpec(player, freighterDesign, recompute: true);
            fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            Assert.AreEqual(75, fleet.Spec.CloakPercent);

        }

        [Test]
        public void TestComputeWMMovement()
        {
            var player = new Player();
            var design = ShipDesigns.LongRangeScount.Clone(player);

            fleetSpecService.ComputeDesignSpec(player, design, recompute: true);
            Assert.AreEqual(4, design.Spec.Movement);

            // WMs get 2 extra movement
            player.Race.PRT = PRT.WM;
            var raceService = TestUtils.TestContainer.GetInstance<RaceService>();
            player.Race.Spec = raceService.ComputeRaceSpecs(player.Race);
            fleetSpecService.ComputeDesignSpec(player, design, recompute: true);
            Assert.AreEqual(6, design.Spec.Movement);
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
            fleetSpecService.ComputeFleetComposition(fleet);
            Assert.IsTrue(fleet.Spec.FleetCompositionComplete);
            Assert.AreEqual(0, fleet.Spec.FleetCompositionTokensRequired.Count);

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
            fleetSpecService.ComputeFleetComposition(fleet);
            Assert.IsFalse(fleet.Spec.FleetCompositionComplete);
            Assert.AreEqual(2, fleet.Spec.FleetCompositionTokensRequired.Count);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Spec.FleetCompositionTokensRequired[0].Purpose);
            Assert.AreEqual(ShipDesignPurpose.Bomber, fleet.Spec.FleetCompositionTokensRequired[0].Purpose);

        }
    }

}