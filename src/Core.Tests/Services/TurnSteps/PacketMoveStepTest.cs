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
using FakeItEasy;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PacketMoveStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PacketMoveStepTest));

        PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
        ShipDesignDiscoverer designDiscoverer = TestUtils.TestContainer.GetInstance<ShipDesignDiscoverer>();

        [Test]
        public void TestCompleteMoveCaught()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, new TestRulesProvider());
            var player = game.Players[0];

            // create a starbase with a warp5 receiver
            var catchingStarbase = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Starbase",
                Hull = Techs.SpaceStation,
                HullSetNumber = 0,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.UltraDriver10, 11, 1),
                }
            };
            var planet2 = new Planet()
            {
                PlayerNum = player.Num,
                Population = 250000,
                Defenses = 50,
                Starbase = new Starbase()
                {
                    PlayerNum = player.Num,
                    Tokens = new List<ShipToken>() {
                        new ShipToken(catchingStarbase, 1)
                    }
                }
            };

            // compute spec for this starbase so the receiver is up to date
            game.Planets.Add(planet2);
            gameRunner.ComputeSpecs();

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player.Num,
                SafeWarpSpeed = 10,
                WarpFactor = 10,
                Cargo = new Cargo(1000),
                Target = planet2
            };

            // make landfall
            step.CompleteMove(packet, player);

            // starbase can accept this packet, should have same defenses/pop, and 1000kT more minerals
            Assert.AreEqual(250000, planet2.Population);
            Assert.AreEqual(50, planet2.Defenses);
            Assert.AreEqual(new Cargo(1000), planet2.Cargo.WithColonists(0));
        }

        [Test]
        public void TestCompleteMoveOverspeed()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, new TestRulesProvider());
            var player = game.Players[0];
            player.TechLevels = new TechLevel(energy: 5);

            // create a starbase with a warp5 receiver
            var catchingStarbase = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Starbase",
                Hull = Techs.SpaceStation,
                HullSetNumber = 0,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.MassDriver5, 11, 1),
                }
            };
            var planet2 = new Planet()
            {
                PlayerNum = player.Num,
                Population = 250000,
                Defenses = 50,
                Starbase = new Starbase()
                {
                    PlayerNum = player.Num,
                    Tokens = new List<ShipToken>() {
                        new ShipToken(catchingStarbase, 1)
                    }
                }
            };

            // compute spec for this starbase so the receiver is up to date
            game.Planets.Add(planet2);
            gameRunner.ComputeSpecs();

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player.Num,
                SafeWarpSpeed = 10,
                WarpFactor = 10,
                Cargo = new Cargo(1000),
                Target = planet2
            };

            // make landfall
            step.CompleteMove(packet, player);

            // 42900 colonists destroyed, 8 defenses destroyed
            Assert.AreEqual(250000 - 42900, planet2.Population);
            Assert.AreEqual(50 - 8, planet2.Defenses);

        }

        [Test]
        public void TestCompleteMoveKillPlanet()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, new TestRulesProvider());
            var player = game.Players[0];

            var planet2 = new Planet()
            {
                PlayerNum = player.Num,
                Population = 250000,
                Defenses = 0,
            };

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player.Num,
                SafeWarpSpeed = 13,
                WarpFactor = 13,
                Cargo = new Cargo(1000),
                Target = planet2
            };

            // make landfall
            step.CompleteMove(packet, player);

            // planet is destroyed
            Assert.AreEqual(0, planet2.Population);
            Assert.AreEqual(0, planet2.Defenses);

        }

        [Test]
        public void TestCompleteMoveDiscoverStarbase()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, new TestRulesProvider());
            var player1 = game.Players[0];
            var player2 = game.Players[1];
            var planet2 = game.Planets[1];

            // create a starbase with a warp5 receiver
            var catchingStarbase = TestUtils.CreateDesign(game, player2, new ShipDesign()
            {
                PlayerNum = player2.Num,
                Name = "Starbase",
                Hull = Techs.SpaceStation,
                HullSetNumber = 0,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.UltraDriver10, 11, 1),
                }
            });
            planet2.Starbase.Tokens = new List<ShipToken>() {
                new ShipToken(catchingStarbase, 1)
            };

            // player1 should detect player2's starbase
            player1.Race.PRT = PRT.PP;

            // compute spec for this starbase so the receiver is up to date
            gameRunner.ComputeSpecs();

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player1.Num,
                SafeWarpSpeed = 10,
                WarpFactor = 10,
                Cargo = new Cargo(1000),
                Target = planet2
            };

            // make landfall
            step.CompleteMove(packet, player1);

            // starbase should be discovered by player1
            player1.SetupMapObjectMappings();
            Assert.IsTrue(player1.DesignIntel.ItemsByGuid.ContainsKey(catchingStarbase.Guid));

            // player2 should catch just fine
            Assert.AreEqual(25000, planet2.Population);
            Assert.AreEqual(new Cargo(1000), planet2.Cargo.WithColonists(0));
        }

        [Test]
        public void TestTerraform()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            IRulesProvider mockRulesProvider = A.Fake<IRulesProvider>();
            var player = game.Players[0];

            // allow Grav3 terraform
            player.Race.PRT = PRT.PP;
            player.TechLevels = new TechLevel(propulsion: 1, biotechnology: 1);
            gameRunner.ComputeSpecs();

            // make sure our random number generator returns "yes, permaform" and "permaform gravity"
            var mockRules = A.Fake<Rules>();
            var random = A.Fake<Random>();
            A.CallTo(() => mockRulesProvider.Rules).Returns(mockRules);
            mockRules.Random = random;

            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, mockRulesProvider);

            // make an extra planet
            var planet2 = new Planet()
            {
                BaseHab = new Hab(47, 50, 50),
                Hab = new Hab(47, 50, 50),
                TerraformedAmount = new Hab(),
            };

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player.Num,
                SafeWarpSpeed = 13,
                WarpFactor = 13,
                Cargo = new Cargo(200),
                Target = planet2
            };

            // set our specs
            player.Race.Spec.PacketTerraformChance = .5f;
            player.Race.Spec.PacketPermaTerraformSizeUnit = 100;

            // make it not-random
            // out of three loops, the first two permaform, the second does not
            // they both permaform gravity
            A.CallTo(() => random.NextDouble()).Returns(.5);
            A.CallTo(() => random.Next(3)).Returns((int)HabType.Gravity);

            step.CheckTerraform(packet, player, planet2, 200);

            // should permaform one step, and adjust our value up one
            Assert.AreEqual(new Hab(49, 50, 50), planet2.Hab);
            Assert.AreEqual(new Hab(47, 50, 50), planet2.BaseHab);
            Assert.AreEqual(new Hab(2, 0, 0), planet2.TerraformedAmount);

            // make it fail
            A.CallTo(() => random.NextDouble()).Returns(.6);
            step.CheckPermaform(packet, player, planet2, 200);
            // should be the same
            Assert.AreEqual(new Hab(49, 50, 50), planet2.Hab);
            Assert.AreEqual(new Hab(47, 50, 50), planet2.BaseHab);
            Assert.AreEqual(new Hab(2, 0, 0), planet2.TerraformedAmount);

        }        

        [Test]
        public void TestPermaform()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            IRulesProvider mockRulesProvider = A.Fake<IRulesProvider>();

            // make sure our random number generator returns "yes, permaform" and "permaform gravity"
            var mockRules = A.Fake<Rules>();
            var random = A.Fake<Random>();
            A.CallTo(() => mockRulesProvider.Rules).Returns(mockRules);
            mockRules.Random = random;

            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService, designDiscoverer, mockRulesProvider);

            // make a PP player
            var player = game.Players[0];
            player.Race.PRT = PRT.PP;

            // make an extra planet
            var planet2 = new Planet()
            {
                BaseHab = new Hab(47, 50, 50),
                Hab = new Hab(47, 50, 50),
                TerraformedAmount = new Hab(),
            };

            // create a 1000kT packet
            MineralPacket packet = new MineralPacket()
            {
                PlayerNum = player.Num,
                SafeWarpSpeed = 13,
                WarpFactor = 13,
                Cargo = new Cargo(200),
                Target = planet2
            };

            // set our specs
            player.Race.Spec.PacketPermaformChance = .001f;
            player.Race.Spec.PacketPermaTerraformSizeUnit = 100;

            // make it not-random
            // out of three loops, the first two permaform, the second does not
            // they both permaform gravity
            A.CallTo(() => random.NextDouble()).Returns(.001);
            A.CallTo(() => random.Next(3)).Returns((int)HabType.Gravity);

            step.CheckPermaform(packet, player, planet2, 200);

            // should permaform one step, and adjust our value up one
            Assert.AreEqual(new Hab(49, 50, 50), planet2.Hab);
            Assert.AreEqual(new Hab(49, 50, 50), planet2.BaseHab);

            // make it fail
            A.CallTo(() => random.NextDouble()).Returns(.002);
            step.CheckPermaform(packet, player, planet2, 200);
            // should be the same
            Assert.AreEqual(new Hab(49, 50, 50), planet2.Hab);
            Assert.AreEqual(new Hab(49, 50, 50), planet2.BaseHab);

        }
    }
}