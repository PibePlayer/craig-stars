using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class RemoteTerraformStepTest
    {
        Game game;
        GameRunner gameRunner;
        PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
        PlayerTechService playerTechService = TestUtils.TestContainer.GetInstance<PlayerTechService>();
        RemoteTerraformStep step;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            step = new RemoteTerraformStep(gameRunner.GameProvider, planetService, playerTechService);
        }

        [Test]
        public void TestRemoteTerraform()
        {
            var terraformer = game.Players[0];
            var player2 = game.Players[1];

            // make them buds (we have to be buds with ourselves as well)
            terraformer.PlayerRelations.Add(new PlayerRelationship(terraformer.Num, PlayerRelation.Friend));
            terraformer.PlayerRelations.Add(new PlayerRelationship(player2.Num, PlayerRelation.Friend));

            // allow TT 
            terraformer.Race.LRTs.Add(LRT.TT);

            // get player2's planet
            var planet = game.Planets[1];
            planet.Hab = new Hab(47, 47, 47);
            planet.BaseHab = new Hab(47, 47, 47);
            planet.TerraformedAmount = new Hab();

            var design = TestUtils.CreateDesign(game, terraformer, ShipDesigns.OrbitalAdjuster);
            var fleet = new Fleet()
            {
                PlayerNum = terraformer.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
                Orbiting = planet,
            };
            planet.OrbitingFleets.Add(fleet);
            game.Fleets.Add(fleet);

            gameRunner.ComputeSpecs(recompute: true);

            // should terraform 2 grav points better
            step.RemoteTerraform(planet, fleet, terraformer, player2);
            Assert.AreEqual(new Hab(48, 48, 47), planet.Hab);
        }

        [Test]
        public void TestRemoteDeterraform()
        {
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            // allow TT 
            player1.Race.LRTs.Add(LRT.TT);

            // get player2's planet
            var planet = game.Planets[1];
            planet.Hab = new Hab(50, 50, 50);
            planet.BaseHab = new Hab(50, 50, 50);
            planet.TerraformedAmount = new Hab();

            var design = TestUtils.CreateDesign(game, player1, ShipDesigns.OrbitalAdjuster);
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
                Orbiting = planet,
            };
            planet.OrbitingFleets.Add(fleet);
            game.Fleets.Add(fleet);

            gameRunner.ComputeSpecs(recompute: true);

            // should de-terraform 2 grav points
            step.RemoteTerraform(planet, fleet, player1, player2);
            Assert.AreEqual(new Hab(52, 50, 50), planet.Hab);

            // should de-terraform 2 more temp points
            // because it's already out of whack
            planet.Hab = new Hab(50, 51, 50);
            planet.TerraformedAmount = new Hab(0, 1, 0);
            step.RemoteTerraform(planet, fleet, player1, player2);
            Assert.AreEqual(new Hab(50, 53, 50), planet.Hab);

            // hittint it again the next turn will de-terraform it by 2
            // grav points
            step.RemoteTerraform(planet, fleet, player1, player2);
            Assert.AreEqual(new Hab(52, 53, 50), planet.Hab);

        }
    }
}