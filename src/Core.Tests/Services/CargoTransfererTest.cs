using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class CargoTransfererTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(CargoTransfererTest));

        [Test]
        public void TestExecuteCargoTransferOrder()
        {
            // create a fleet with some cargo
            var source = new Fleet()
            {
                Cargo = new Cargo(10, 20, 30, 40),
                Fuel = 50
            };
            source.Spec.CargoCapacity = 120;
            source.Spec.FuelCapacity = 100;

            var dest = new Planet()
            {
                Cargo = new Cargo(100, 200, 300, 400)
            };

            var cargoTransferer = new CargoTransferer();

            // transfer some cargo from the source to the dest
            var result = cargoTransferer.Transfer(source, dest, new Cargo(5, 10, 15, 20), 0);
            Assert.AreEqual(new Cargo(5, 10, 15, 20), source.Cargo);
            Assert.AreEqual(50, source.Fuel); // fuel should be unchanged
            Assert.AreEqual(new Cargo(105, 210, 315, 420), dest.Cargo);
            Assert.AreEqual(new Cargo(-5, -10, -15, -20), result.cargo);

        }

        [Test]
        public void TestExecuteCargoTransferOrderTooMuch()
        {
            // create a fleet with some cargo
            var source = new Fleet()
            {
                Cargo = new Cargo(10, 20, 30, 40),
                Fuel = 50
            };
            source.Spec.CargoCapacity = 120;
            source.Spec.FuelCapacity = 100;

            var dest = new Planet()
            {
                Cargo = new Cargo(100, 200, 300, 400)
            };

            var cargoTransferer = new CargoTransferer();

            // try and transfer more that possible from the source to the dest
            // we should transfer all available
            var result = cargoTransferer.Transfer(source, dest, new Cargo(20, 30, 40, 50), 0);
            Assert.AreEqual(new Cargo(0, 0, 0, 0), source.Cargo);
            Assert.AreEqual(50, source.Fuel); // fuel should be unchanged
            Assert.AreEqual(new Cargo(110, 220, 330, 440), dest.Cargo);
            Assert.AreEqual(new Cargo(-10, -20, -30, -40), result.cargo);
        }

        [Test]
        public void TestExecuteCargoTransferOrderNotEnoughRoom()
        {
            var sourcePlanet = new Planet()
            {
                Cargo = new Cargo(100, 200, 300, 400)
            };

            // create an empty fleet with limited capacity
            var destFleet = new Fleet()
            {
                Cargo = new Cargo(),
                Fuel = 50
            };
            destFleet.Spec.CargoCapacity = 120;
            destFleet.Spec.FuelCapacity = 100;


            var cargoTransferer = new CargoTransferer();

            // try and transfer more than the destFleet can hold
            // it should take as much as possible
            var result = cargoTransferer.Transfer(sourcePlanet, destFleet, new Cargo(20, 30, 40, 50), 0);
            Assert.AreEqual(new Cargo(20, 30, 40, 30), destFleet.Cargo);
            Assert.AreEqual(new Cargo(80, 170, 260, 370), sourcePlanet.Cargo);
            Assert.AreEqual(new Cargo(-20, -30, -40, -30), result.cargo);
        }
    }
}
