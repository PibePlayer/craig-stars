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
using CraigStars.UniverseGeneration;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerScanStepTest
    {
        static ILog log = LogManager.GetLogger(typeof(PlayerScanStepTest));

        [Test]
        public void TestScan()
        {
            var game = GameTest.GetSingleUnitGame();

            // setup initial player knowledge
            var ug = new UniverseGenerator(game);
            ug.Generate();

            game.Planets[0].Population = 120000;

            game.UpdateDictionaries();
            game.UpdatePlayers();

            var scanStep = new PlayerScanStep(game);
            scanStep.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // our player should know about the planet updates
            Assert.AreEqual(game.Planets[0].Population, game.Players[0].Planets[0].Population);
            Assert.IsTrue(game.Players[0].Fleets.Count > 0);
        }
    }

}