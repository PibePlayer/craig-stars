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
            var game = new Game();
            var rules = new Rules(0);
            game.Init(new List<Player>() { new Player() }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

            Assert.AreEqual(rules.GetNumPlanets(game.Size, game.Density), game.Planets.Count);
            Assert.AreEqual(rules.GetNumPlanets(game.Size, game.Density), game.Players[0].AllPlanets.ToList().Count);
            Assert.AreEqual(game.Fleets.Count, game.Players[0].Fleets.Count);
        }

        [Test]
        public void TestGenerateTurn()
        {
            // create a new game with universe
            var game = new Game();
            var player = new Player();
            var rules = new Rules(0);
            game.Init(new List<Player>() { player }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

            // submit the player
            game.SubmitTurn(player);

            // generate the turn
            game.GenerateTurn();

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
            // create a new game with universe
            var game = new Game()
            {
                Size = Size.Huge,
                Density = Density.Packed,
            };
            game.GameInfo.QuickStartTurns = 0;
            var player = new Player() { Num = 0 };
            var aiPlayer = new Player() { Num = 1, AIControlled = true };
            var rules = new Rules(0);
            game.Init(new List<Player>() { player, aiPlayer }, rules, StaticTechStore.Instance);
            game.GenerateUniverse();

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
                game.SubmitTurn(player);

                // generate the turn
                game.GenerateTurn();
            }
            stopwatch.Stop();

            // turn back on logging defaults
            // logger.Hierarchy.Root.Level = Level.All;
            log.Debug($"Generated {numTurns} turns in {stopwatch.ElapsedMilliseconds / 1000.0f} seconds");

            // make sure our turn was generated and the player's report was updated
            Assert.Greater(player.Homeworld.Population, rules.StartingPopulation);
            Assert.AreEqual(0, player.Homeworld.ReportAge);
            Assert.AreEqual(rules.StartingYear + numTurns, game.Year);
        }

    }

}