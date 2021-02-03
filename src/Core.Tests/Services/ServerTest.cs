using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ServerTest
    {
        static ILog log = LogManager.GetLogger(typeof(ServerTest));

        /// <summary>
        /// Test helper method to return a simple server
        /// </summary>
        /// <returns>A server with one planet, one player, one fleet</returns>
        Server GetSingleUnitServer()
        {
            var server = new Server();
            server.TechStore = StaticTechStore.Instance;
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
        public void TestGenerateUniverse()
        {
            var server = new Server();
            var rules = new Rules();
            server.Init(new List<Player>() { new Player() }, rules, StaticTechStore.Instance);
            server.GenerateUniverse();

            Assert.AreEqual(rules.NumPlanets, server.Planets.Count);
            Assert.AreEqual(rules.NumPlanets, server.Players[0].Planets.Count);
            Assert.AreEqual(server.Fleets.Count, server.Players[0].Fleets.Count);
        }

        [Test]
        public void TestGenerateTurn()
        {
            // create a new server with universe
            var server = new Server();
            var player = new Player();
            var rules = new Rules();
            server.Init(new List<Player>() { player }, rules, StaticTechStore.Instance);
            server.GenerateUniverse();

            // submit the player
            server.SubmitTurn(player);

            // generate the turn
            server.GenerateTurn();

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Homeworld.Population, rules.StartingPopulation);
            Assert.AreEqual(0, player.Homeworld.ReportAge);
        }

        /// <summary>
        /// Test generating multiple turns with an AI and Human player
        /// </summary>
        [Test]
        public void TestGenerateManyTurns()
        {
            // create a new server with universe
            var server = new Server();
            var player = new Player();
            var aiPlayer = new Player() { AIControlled = true };
            var rules = new Rules()
            {
                Size = Size.Huge,
                Density = Density.Packed
            };
            server.Init(new List<Player>() { player, aiPlayer }, rules, StaticTechStore.Instance);
            server.GenerateUniverse();

            // turn off logging but for errors
            var logger = (Logger)log.Logger;
            logger.Hierarchy.Root.Level = Level.Error;

            // generate a thousand turns
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int numTurns = 10; //1000;

            // generate a bunch of turns
            for (int i = 0; i < numTurns; i++)
            {
                // submit the player
                server.SubmitTurn(player);

                // generate the turn
                server.GenerateTurn();
            }
            stopwatch.Stop();

            // turn back on logging defaults
            logger.Hierarchy.Root.Level = Level.All;
            log.Debug($"Generated {numTurns} turns in {stopwatch.ElapsedMilliseconds / 1000.0f} seconds");

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Homeworld.Population, rules.StartingPopulation);
            Assert.AreEqual(0, player.Homeworld.ReportAge);
            Assert.AreEqual(rules.StartingYear + numTurns, server.Year);
        }
    }

}