using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TurnGeneratorTest
    {
        /// <summary>
        /// Test helper method to return a simple server
        /// </summary>
        /// <returns>A server with one planet, one player, one fleet</returns>
        Server GetSingleUnitServer()
        {
            var server = new Server();
            server.TechStore = TechStore.Instance;
            var player = new Player();
            server.Players.Add(player);

            server.Planets.Add(new Planet()
            {
                Player = player,

                Cargo = new Cargo(),
                MineYears = new Mineral(),
                ProductionQueue = new ProductionQueue(),
                Population = 100000,
                Hab = new Hab(50, 50, 50),
                MineralConcentration = new Mineral(100, 100, 100),
                Scanner = true
            });
            server.Fleets.Add(new Fleet()
            {
                Player = player,
                Tokens = new List<ShipToken>(new ShipToken[] {
                    new ShipToken()
                    {
                        Design = ShipDesigns.LongRangeScount,
                        Quantity = 1
                    }
                })
            });
            return server;
        }
        [Test]
        public void TestGenerateTurn()
        {
            var server = GetSingleUnitServer();
            var tg = new TurnGenerator(server);
            tg.GenerateTurn();
            tg.RunTurnProcessors();

            // make sure our planet grew pop
            Assert.IsTrue(server.Planets[0].Population > 100000);

        }

        [Test]
        public void TestUpdatePlayerReports()
        {
            var server = GetSingleUnitServer();

            // setup initial player knowledge
            var ug = new UniverseGenerator(server);
            ug.InitPlayerReports(server.Players[0], server.Planets);

            var tg = new TurnGenerator(server);
            server.Planets[0].Population = 120000;
            tg.UpdatePlayerReports();

            // our player should know about the planet updates
            Assert.AreEqual(server.Planets[0].Population, server.Players[0].Planets[0].Population);
            Assert.IsTrue(server.Players[0].Fleets.Count > 0);
        }

    }

}