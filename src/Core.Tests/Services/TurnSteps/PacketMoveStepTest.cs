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
    public class PacketMoveStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PacketMoveStepTest));

        PlanetService planetService = new PlanetService(new PlayerTechService(new TestTechStoreProvider()));

        [Test]
        public void TestCompleteMoveCaught()
        {
            var (game, gameRunner) = TestUtils.GetSingleUnitGame();
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService);
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

            // compute aggregate for this starbase so the receiver is up to date
            planet2.Starbase.ComputeAggregate(player);

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
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService);
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

            // compute aggregate for this starbase so the receiver is up to date
            planet2.Starbase.ComputeAggregate(player);

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
            PacketMove1Step step = new PacketMove1Step(gameRunner.GameProvider, planetService);
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
    }
}