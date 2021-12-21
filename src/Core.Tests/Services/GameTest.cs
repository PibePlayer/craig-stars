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
        static CSLog log = LogProvider.GetLogger(typeof(GameTest));

        [Test]
        public void TestGenerateUniverse()
        {
            var gameRunner = GameRunnerContainer.CreateGameRunner(new Game(), StaticTechStore.Instance);
            var game = gameRunner.Game;
            var rules = new Rules(0);
            game.Init(new List<Player>() { new Player() }, rules);
            gameRunner.GenerateUniverse();

            Assert.AreEqual(rules.GetNumPlanets(game.Size, game.Density), game.Planets.Count);
            Assert.AreEqual(rules.GetNumPlanets(game.Size, game.Density), game.Players[0].AllPlanets.ToList().Count);
            Assert.AreEqual(game.Fleets.Count, game.Players[0].Fleets.Count);
        }

        [Test]
        public void TestGenerateTurn()
        {
            // create a new game with universe
            var gameRunner = GameRunnerContainer.CreateGameRunner(new Game(), StaticTechStore.Instance);
            var game = gameRunner.Game;
            var player = new Player();
            var rules = new Rules(0);
            game.Init(new List<Player>() { player }, rules);
            gameRunner.GenerateUniverse();

            // submit the player
            gameRunner.SubmitTurn(player.GetOrders());

            // generate the turn
            gameRunner.GenerateTurn();
            gameRunner.ComputeSpecs();

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(game.Planets[0].Population, 25000);
            Assert.AreEqual(0, player.Planets[0].ReportAge);
        }

        /// <summary>
        /// Test generating multiple turns with an AI and Human player
        /// </summary>
        [Test]
        public void TestGenerateManyTurns()
        {
            // create a new game with universe
            var gameRunner = GameRunnerContainer.CreateGameRunner(new Game()
            {
                Size = Size.Huge,
                Density = Density.Packed,
            }, StaticTechStore.Instance);
            var game = gameRunner.Game;
            game.GameInfo.QuickStartTurns = 0;
            var player = new Player() { Num = 0 };
            var aiPlayer = new Player() { Num = 1, AIControlled = true };
            var rules = new Rules(0);
            game.Init(new List<Player>() { player, aiPlayer }, rules);
            gameRunner.GenerateUniverse();

            // // turn off logging but for errors
            // var logger = (Logger)log.Logger;
            // logger.Hierarchy.Root.Level = Level.Error;

            // generate a thousand turns
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int numTurns = 10; //1000;

            var aiTurnSubmitter = new AITurnSubmitter(new TestTurnProcessorManager());

            // generate a bunch of turns
            for (int i = 0; i < numTurns; i++)
            {
                aiTurnSubmitter.SubmitAITurns(game);

                // submit the player
                gameRunner.SubmitTurn(player.GetOrders());

                // generate the turn
                gameRunner.GenerateTurn();
            }
            stopwatch.Stop();

            // turn back on logging defaults
            // logger.Hierarchy.Root.Level = Level.All;
            log.Debug($"Generated {numTurns} turns in {stopwatch.ElapsedMilliseconds / 1000.0f} seconds");

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Planets[0].Population, 25000);
            Assert.AreEqual(0, player.Planets[0].ReportAge);
            Assert.AreEqual(rules.StartingYear + numTurns, game.Year);
        }

    }

}