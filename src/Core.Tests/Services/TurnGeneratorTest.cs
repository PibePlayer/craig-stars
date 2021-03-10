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
        /// Test helper method to return a simple game
        /// </summary>
        /// <returns>A game with one planet, one player, one fleet</returns>
        Game GetSingleUnitGame()
        {
            var game = new Game();
            game.TechStore = StaticTechStore.Instance;
            var player = new Player();
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
            game.Fleets.Add(new Fleet()
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

            // make sure the player has a copy of this design
            player.Designs.Add(ShipDesigns.LongRangeScount.Clone());

            return game;
        }
        [Test]
        public void TestGenerateTurn()
        {
            var game = GetSingleUnitGame();
            var tg = new TurnGenerator(game);
            tg.GenerateTurn();
            tg.RunTurnProcessors();

            // make sure our planet grew pop
            Assert.IsTrue(game.Planets[0].Population > 100000);

        }

        [Test]
        public void TestUpdatePlayerReports()
        {
            var game = GetSingleUnitGame();

            // setup initial player knowledge
            var ug = new UniverseGenerator(game);
            ug.InitPlayerReports(game.Players[0], game.Planets);

            var tg = new TurnGenerator(game);
            game.Planets[0].Population = 120000;
            tg.UpdatePlayerReports();

            // our player should know about the planet updates
            Assert.AreEqual(game.Planets[0].Population, game.Players[0].Planets[0].Population);
            Assert.IsTrue(game.Players[0].Fleets.Count > 0);
        }

    }

}