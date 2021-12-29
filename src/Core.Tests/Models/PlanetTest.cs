using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetTest
    {

        [Test]
        public void TestTransfer()
        {
            var planet = new Planet();
            planet.Cargo = new Cargo(10, 20, 30);

            // give a planet cargo, but it won't take fuel
            var result = planet.Transfer(new Cargo(ironium: 20, boranium: 10), 10);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 20, boranium: 10), 0), result);
            Assert.AreEqual(new Cargo(30, 30, 30), planet.Cargo);

            // can only take away cargo, but only down to 0
            planet.Cargo = new Cargo(10, 20, 30);
            result = planet.Transfer(new Cargo(ironium: -100, boranium: -10), 0);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: -10, boranium: -10), 0), result);
            Assert.AreEqual(new Cargo(0, 10, 30), planet.Cargo);

        }
    }

}