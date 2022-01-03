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

        [Test]
        public void TestCanTransfer()
        {
            var cargoTransferer = new CargoTransferer();
            var sourceFleet = new Fleet() { PlayerNum = 0 };
            var destFleetPlayer1 = new Fleet() { PlayerNum = 0 };
            var destPlanetPlayer1 = new Planet() { PlayerNum = 0 };
            var destFleetPlayer2 = new Fleet() { PlayerNum = 1 };
            var destPlanetPlayer2 = new Planet() { PlayerNum = 1 };
            var destSalvage = new Salvage();
            var destMineralPacketPlayer2 = new MineralPacket() { PlayerNum = 2 };

            var mapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

            // var mapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>() {
            //     { sourceFleet.Position, new List<MapObject>() {
            //         sourceFleet,
            //         destFleetPlayer1
            //     }}
            // };


            Assert.IsTrue(cargoTransferer.CanTransfer(sourceFleet, destFleetPlayer1, mapObjectsByLocation));
            Assert.IsTrue(cargoTransferer.CanTransfer(sourceFleet, destPlanetPlayer1, mapObjectsByLocation));
            Assert.IsFalse(cargoTransferer.CanTransfer(sourceFleet, destFleetPlayer2, mapObjectsByLocation));
            Assert.IsFalse(cargoTransferer.CanTransfer(sourceFleet, destPlanetPlayer2, mapObjectsByLocation));

            // anyone can transfer from mineral packets
            Assert.IsTrue(cargoTransferer.CanTransfer(sourceFleet, destSalvage, mapObjectsByLocation));
            Assert.IsTrue(cargoTransferer.CanTransfer(sourceFleet, destMineralPacketPlayer2, mapObjectsByLocation));

            // if we are in orbit with a cargo stealing fleet, we can steal
            var cargoStealingFleet = new Fleet() { PlayerNum = 0 };
            cargoStealingFleet.Spec.CanStealFleetCargo = true;
            mapObjectsByLocation[sourceFleet.Position] = new List<MapObject>() {
                sourceFleet,
                cargoStealingFleet,
            };

            // check fleet stealer
            Assert.IsTrue(cargoTransferer.CanTransfer(cargoStealingFleet, destFleetPlayer2, mapObjectsByLocation));
            Assert.IsFalse(cargoTransferer.CanTransfer(cargoStealingFleet, destPlanetPlayer2, mapObjectsByLocation));

            // check planet stealing
            cargoStealingFleet.Spec.CanStealPlanetCargo = true;
            Assert.IsTrue(cargoTransferer.CanTransfer(cargoStealingFleet, destPlanetPlayer2, mapObjectsByLocation));

        }
    }
}
