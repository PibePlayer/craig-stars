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
    public class CalculateScoreStepTest
    {
        PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();

        [Test]
        public void TestCalculateScoreSinglePlanet()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            // single planet and fleet, 2 points (1 for planet, .5 rounded up for fleet)
            // also 3 points for starbase
            var step = new CalculateScoreStep(gameRunner.GameProvider, planetService);
            var score = step.CalculateScore(player);

            Assert.AreEqual(5, score.Score);
        }

        [Test]
        public void TestCalculateScoreSimple()
        {
            // Give the player the following points:
            // planets = 2 * 1pt = 2pt
            // resources (planet 1) = 25 (pop) + 10 (factories) = 3 pts
            // resources (planet 2) = 210 (pop) = 7 pts
            // population (planet 1) <100k = 1 pts
            // population (planet 2) 100k to 200k = 2 pts
            // population (planet 2) 100k to 200k = 2 pts
            // starbases = 1 * 3pt = 3pts
            // fleet1 = 1 pts
            // fleet2 = 5 pts
            // tech levels = 10 pts
            // total 31 pts
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            game.Planets[0].Population = 25000; // 25 resources
            game.Planets[0].Factories = 10; // 10 resources

            // should be 3 points for a starbase
            var starbaseDesign = ShipDesigns.Starbase.Clone(player);
            game.Designs.Add(starbaseDesign);
            game.Planets[0].Starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>()
                {
                    new ShipToken() { Quantity = 1, Design = starbaseDesign
                }
            }
            };

            // should be 1 more point for an extra planet, and this planet 
            // generates 7 pts for 210 resources
            game.Planets.Add(new Planet()
            {
                PlayerNum = player.Num,
                Population = 210000,  // should be 2 points for population, 210 resources for 7 more points
            });
            game.Fleets.Add(game.Fleets[0]);
            game.Fleets[0].Tokens[0].Quantity = 2; // should be one point for 2 small tokens

            // add a capital ship for 5 points (with 2 planets) (8 * 2 / 3)
            game.Fleets.Add(new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken()
                    {
                        Quantity = 1,
                        Design = TestUtils.CreateDesign(game, player, new ShipDesign()
                        {
                            PlayerNum = player.Num,
                            Hull = Techs.Battleship,
                            Slots = new List<ShipDesignSlot>()
                            {
                                new ShipDesignSlot(Techs.GalaxyScoop, 1, 4),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 4, 8),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 5, 6),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 6, 6),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 7, 2),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 8, 2),
                                new ShipDesignSlot(Techs.UpsilonTorpedo, 8, 4)
                            },
                        }),
                    }
                }
            });

            player.TechLevels = new TechLevel(
                1, 4, 7, 10 // should be 1 + 2 + 3 + 4 = 10 points total
            );

            gameRunner.ComputeAggregates(recompute: true);
            var step = new CalculateScoreStep(gameRunner.GameProvider, planetService);
            var score = step.CalculateScore(player);

            Assert.AreEqual(31, score.Score);
        }

    }
}