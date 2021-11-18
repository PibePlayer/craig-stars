using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using CraigStars.UniverseGeneration;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerShipDesignsGenerationStepTest
    {

        PlayerTechService playerTechService;
        PlayerIntel playerIntel;
        ShipDesignGenerator designGenerator;
        FleetSpecService fleetSpecService;

        [SetUp]
        public void SetUp()
        {
            playerTechService = TestUtils.TestContainer.GetInstance<PlayerTechService>();
            playerIntel = TestUtils.TestContainer.GetInstance<PlayerIntel>();
            designGenerator = TestUtils.TestContainer.GetInstance<ShipDesignGenerator>();
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
        }

        [Test]
        public void TestFillStarbaseSlots()
        {
            var game = new Game() { StartMode = GameStartMode.Normal };
            game.Init(new List<Player>() { new Player() { AIControlled = true } }, new Rules(0));

            PlayerShipDesignsGenerationStep step = new PlayerShipDesignsGenerationStep(
                new Provider<Game>(game),
                StaticTechStore.Instance,
                playerIntel,
                designGenerator,
                fleetSpecService
            );

            var starbase = new ShipDesign()
            {
                PlayerNum = game.Players[0].Num,
                Hull = Techs.SpaceStation
            };

            step.FillStarbaseSlots(starbase, new Race(), new StartingPlanet(0, 0));

            // slot 1 is an orbital, should be empty for Humanoids

            // slot 2 should be a laser
            Assert.AreEqual(2, starbase.Slots[0].HullSlotIndex);
            Assert.AreEqual(8, starbase.Slots[0].Quantity);
            Assert.AreEqual(Techs.Laser, starbase.Slots[0].HullComponent);

            // slot 3 should be a shield
            Assert.AreEqual(3, starbase.Slots[1].HullSlotIndex);
            Assert.AreEqual(8, starbase.Slots[1].Quantity);
            Assert.AreEqual(Techs.MoleSkinShield, starbase.Slots[1].HullComponent);

        }
    }
}
