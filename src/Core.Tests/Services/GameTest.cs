using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Threading.Tasks;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class GameTest
    {
        static ILog log = LogManager.GetLogger(typeof(GameTest));

        /// <summary>
        /// Test helper method to return a simple game
        /// </summary>
        /// <returns>A game with one planet, one player, one fleet</returns>
        internal static Game GetSingleUnitGame()
        {
            var game = new Game()
            {
                SaveToDisk = false,
                TechStore = StaticTechStore.Instance
            };
            var player = new Player()
            {
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            player.SetupMapObjectMappings();
            game.Players.Add(player);

            game.Planets.Add(new Planet()
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

            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = player;
            game.Designs.Add(design);

            game.Fleets.Add(new Fleet()
            {
                Player = player,
                Tokens = new List<ShipToken>(new ShipToken[] {
                    new ShipToken()
                    {
                        Design = design,
                        Quantity = 1
                    }
                }),
                BattlePlan = player.BattlePlans[0]
            });
            return game;
        }

        [Test]
        public void TestGenerateUniverse()
        {
            var game = new Game() { SaveToDisk = false };
            var rules = new Rules(0);
            game.Init(new List<Player>() { new Player() }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

            Assert.AreEqual(rules.NumPlanets, game.Planets.Count);
            Assert.AreEqual(rules.NumPlanets, game.Players[0].AllPlanets.ToList().Count);
            Assert.AreEqual(game.Fleets.Count, game.Players[0].Fleets.Count);
        }

        [Test]
        public async Task TestGenerateTurn()
        {
            // create a new game with universe
            var game = new Game() { SaveToDisk = false };
            var player = new Player();
            var rules = new Rules(0);
            game.Init(new List<Player>() { player }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

            // submit the player
            game.SubmitTurn(player);

            // generate the turn
            await game.GenerateTurn();

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Homeworld.Population, rules.StartingPopulation);
            Assert.AreEqual(0, player.Homeworld.ReportAge);
        }

        /// <summary>
        /// Test generating multiple turns with an AI and Human player
        /// </summary>
        [Test]
        public async Task TestGenerateManyTurns()
        {
            // create a new game with universe
            var game = new Game() { SaveToDisk = false };
            var player = new Player();
            var aiPlayer = new Player() { AIControlled = true };
            var rules = new Rules(0)
            {
                Size = Size.Huge,
                Density = Density.Packed
            };
            game.Init(new List<Player>() { player, aiPlayer }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

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
                game.SubmitTurn(player);

                // generate the turn
                await game.GenerateTurn();
            }
            stopwatch.Stop();

            // turn back on logging defaults
            logger.Hierarchy.Root.Level = Level.All;
            log.Debug($"Generated {numTurns} turns in {stopwatch.ElapsedMilliseconds / 1000.0f} seconds");

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Homeworld.Population, rules.StartingPopulation);
            Assert.AreEqual(0, player.Homeworld.ReportAge);
            Assert.AreEqual(rules.StartingYear + numTurns, game.Year);
        }

    }

}