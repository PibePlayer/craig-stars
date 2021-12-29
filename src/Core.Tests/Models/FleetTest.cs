using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetTest
    {

        [Test]
        public void TestTransfer()
        {
            var fleet = new Fleet();
            fleet.Spec.CargoCapacity = 100;
            fleet.Spec.FuelCapacity = 50;

            // transfer some cargo and fuel
            var result = fleet.Transfer(new Cargo(ironium: 25), 10);

            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 25), 10), result);
            Assert.AreEqual(new Cargo(ironium: 25), fleet.Cargo);
            Assert.AreEqual(10, fleet.Fuel);
        }

        [Test]
        public void TestTransferOver()
        {
            var fleet = new Fleet();
            fleet.Spec.CargoCapacity = 100;
            fleet.Spec.FuelCapacity = 50;

            // transfer some cargo and fuel
            var result = fleet.Transfer(new Cargo(ironium: 125), 60);

            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 100), 50), result);
            Assert.AreEqual(new Cargo(ironium: 100), fleet.Cargo);
            Assert.AreEqual(50, fleet.Fuel);
        }

        [Test]
        public void TestTransferOverAddToCargo()
        {
            var fleet = new Fleet();
            fleet.Spec.CargoCapacity = 120;
            fleet.Spec.FuelCapacity = 50;
            fleet.Cargo = new Cargo(10, 20, 30, 40); // 100 total, 20 left in capacity
            fleet.Fuel = 10;

            // transfer too much ironium and fuel
            var result = fleet.Transfer(new Cargo(ironium: 100), 60);

            // should transfer 20 ironium, 40 fuel (filling up completely)
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 20), 40), result);
            Assert.AreEqual(new Cargo(30, 20, 30, 40), fleet.Cargo);
            Assert.AreEqual(50, fleet.Fuel);
        }

        [Test]
        public void TestTransferUnder()
        {
            var fleet = new Fleet();
            fleet.Spec.CargoCapacity = 100;
            fleet.Spec.FuelCapacity = 50;

            fleet.Cargo = new Cargo(10, 20, 30, 40);
            fleet.Fuel = 50;

            // transfer some cargo and fuel
            var result = fleet.Transfer(new Cargo(ironium: -125), -60);

            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: -10), -50), result);
            Assert.AreEqual(new Cargo(0, 20, 30, 40), fleet.Cargo);
            Assert.AreEqual(0, fleet.Fuel);
        }

        [Test]
        public void TestTransferComplex()
        {
            var fleet = new Fleet();
            fleet.Spec.CargoCapacity = 120;
            fleet.Spec.FuelCapacity = 50;

            fleet.Cargo = new Cargo(10, 20, 30, 40); // 100 total, 20 extra capacity
            fleet.Fuel = 40; // 10 extra capacity

            // take 10 ironium away (try to take 20)
            // give 40 boranium, only 30 should be accepted because we have 20 room + 10 extra room from the lost ironium
            // give 20 fuel, only 10 should be accepted
            var result = fleet.Transfer(new Cargo(ironium: -20, boranium: 40), 20);

            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: -10, boranium: 30), 10), result);
            Assert.AreEqual(new Cargo(0, 50, 30, 40), fleet.Cargo);
            Assert.AreEqual(50, fleet.Fuel);
        }
    }

}