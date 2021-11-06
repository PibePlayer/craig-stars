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

        [Test]
        public void TestCalculateScoreSinglePlanet()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];

            // single planet and fleet, 3 points
            var step = new CalculateScoreStep(game);
            var score = step.CalculateScore(player);

            Assert.AreEqual(0, score.Score);
        }

        [Test]
        public void TestCalculateScoreSimple()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player = game.Players[0];
            var starbaseDesign = ShipDesigns.Starbase.Clone(player);


            player.Planets.Add(game.Planets[0]); // should be one point
            player.Planets[0].Factories = 10; // should be one point for 30 resources
            player.Planets[0].Starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                new ShipToken() { Quantity = 1, Design = starbaseDesign }
            }
            }; // should be 3 points for a starbase
            player.Planets.Add(new Planet()
            {
                PlayerNum = player.Num,
                Population = 210000,  // should be 2 points for population, 210 resources for 7 more points
            });
            player.Fleets.Add(game.Fleets[0]);
            player.Fleets[0].Tokens[0].Quantity = 2; // should be one point

            // add a capital ship for 5 points (with 2 planets) (8 * 2 / 3)
            player.Fleets.Add(new Fleet()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken()
                    {
                        Quantity = 1,
                        Design = new ShipDesign()
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
                        },
                    }
                }
            });

            player.TechLevels = new TechLevel(
                1, 4, 7, 10 // should be 1 + 2 + 3 + 4 = 10 points total
            );

            // empty player, no score
            player.ComputeAggregates();
            var step = new CalculateScoreStep(game);
            var score = step.CalculateScore(player);

            Assert.AreEqual(30, score.Score);
        }

    }
}