using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetOrderExecutorTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetOrderExecutorTest));

        FleetService fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
        CargoTransferer cargoTransferer = TestUtils.TestContainer.GetInstance<CargoTransferer>();
        FleetOrderExecutor orderExecutor;
        Game game;
        GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            orderExecutor = new FleetOrderExecutor(game, fleetService, cargoTransferer);
        }

        [Test]
        public void TestExecuteCargoTransferOrder()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];

            planet.Cargo = new Cargo(ironium: 300);
            
            fleet.Spec.CargoCapacity = 100;
            fleet.Cargo = new Cargo();

            // transfer 50 ironium to the fleet from the planet
            var order = new CargoTransferOrder() {
                Guid = fleet.Guid,
                DestGuid = planet.Guid,
                Transfer = new Cargo(ironium: -50)
            };

            orderExecutor.ExecuteCargoTransferOrder(player, order);
            Assert.AreEqual(new Cargo(ironium: 50), fleet.Cargo);
            Assert.AreEqual(new Cargo(ironium: 250), planet.Cargo);
        }

        [Test]
        public void TestExecuteCargoTransferOrderTooMuch()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];

            planet.Cargo = new Cargo(ironium: 300);
            
            fleet.Spec.CargoCapacity = 100;
            fleet.Cargo = new Cargo();

            // try and transfer 200 ironium to the fleet from the planet
            var order = new CargoTransferOrder() {
                Guid = fleet.Guid,
                DestGuid = planet.Guid,
                Transfer = new Cargo(ironium: -200)
            };

            orderExecutor.ExecuteCargoTransferOrder(player, order);
            Assert.AreEqual(new Cargo(ironium: 100), fleet.Cargo);
            Assert.AreEqual(new Cargo(ironium: 200), planet.Cargo);
        }

        [Test]
        public void TestExecuteCargoTransferOrderNotEnoughOnPlanet()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            var fleet = game.Fleets[0];

            planet.Cargo = new Cargo(ironium: 50);
            
            fleet.Spec.CargoCapacity = 100;
            fleet.Cargo = new Cargo();

            // try and transfer 100 ironium to the fleet with only 50 on the planet
            var order = new CargoTransferOrder() {
                Guid = fleet.Guid,
                DestGuid = planet.Guid,
                Transfer = new Cargo(ironium: -100)
            };

            orderExecutor.ExecuteCargoTransferOrder(player, order);
            Assert.AreEqual(new Cargo(ironium: 50), fleet.Cargo);
            Assert.AreEqual(new Cargo(ironium: 0), planet.Cargo);
        }        
    }
}
