using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ShipDesignerTurnProcessorTest
    {
        ShipDesignerTurnProcessor shipDesignerTurnProcessor;

        [SetUp]
        public void SetUp()
        {
            shipDesignerTurnProcessor = new ShipDesignerTurnProcessor(
                TestUtils.TestContainer.GetInstance<ShipDesignGenerator>(),
                TestUtils.TestContainer.GetInstance<PlayerTechService>(),
                TestUtils.TestContainer.GetInstance<FleetAggregator>(),
                TestUtils.TestContainer.GetInstance<ITechStore>()
                );
        }

        [Test]
        public void TestDesignColonyShip()
        {
            var player = new Player()
            {
                TechLevels = new TechLevel(3, 3, 3, 3, 3, 3)
            };

            var design = shipDesignerTurnProcessor.DesignColonizer(player);

            Assert.AreEqual(Techs.MediumFreighter, design.Hull);
            Assert.AreEqual(Techs.ColonizationModule, design.Slots[1].HullComponent);

        }
    }
}