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

        [Test]
        public void TestFillStarbaseSlots()
        {
            var game = new Game() { SaveToDisk = false, StartMode = GameStartMode.Normal };
            game.Init(new List<Player>() { new Player() { AIControlled = true } }, new Rules(0), StaticTechStore.Instance, new TestGamesManager(), new TestTurnProcessorManager());

            PlayerShipDesignsGenerationStep step = new PlayerShipDesignsGenerationStep(game);

            var starbase = new ShipDesign()
            {
                Player = game.Players[0],
                Hull = Techs.SpaceStation
            };

            step.FillStarbaseSlots(starbase, new Race());

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
