using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ShipDesignGeneratorTest
    {

        [Test]
        public void TestDesignShip()
        {
            var player = new Player();
            var techStore = StaticTechStore.Instance;

            // design a simple scout
            var designer = new ShipDesignGenerator();
            var design = designer.DesignShip(Techs.Scout, "Name", player, 0);
            Assert.IsNotNull(design);
            Assert.AreEqual(Techs.QuickJump5, design.Slots[0].HullComponent);
            Assert.AreEqual(Techs.BatScanner, design.Slots[1].HullComponent);
            Assert.AreEqual(Techs.FuelTank, design.Slots[2].HullComponent);

            // design a colony ship
            design = designer.DesignShip(Techs.ColonyShip, "Name", player, 0);
            Assert.AreEqual(Techs.QuickJump5, design.Slots[0].HullComponent);
            Assert.AreEqual(Techs.ColonizationModule, design.Slots[1].HullComponent);

            // design a starbase
            design = designer.DesignShip(Techs.SpaceStation, "Name", player, 0);
            Assert.IsNotNull(design);

        }
    }

}