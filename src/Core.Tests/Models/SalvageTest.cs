using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class SalvageTest
    {

        [Test]
        public void TestTransfer()
        {
            var salvage = new Salvage();
            salvage.Cargo = new Cargo(10, 20, 30);

            // can't give a salvage cargo
            var result = salvage.Transfer(new Cargo(ironium: 25), 10);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 0), 0), result);
            Assert.AreEqual(new Cargo(10, 20, 30), salvage.Cargo);

            // can only take away cargo, but only down to 0
            salvage.Cargo = new Cargo(10, 20, 30);
            result = salvage.Transfer(new Cargo(ironium: -20, boranium: -10), 0);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: -10, boranium: -10), 0), result);
            Assert.AreEqual(new Cargo(0, 10, 30), salvage.Cargo);
        }
    }

}