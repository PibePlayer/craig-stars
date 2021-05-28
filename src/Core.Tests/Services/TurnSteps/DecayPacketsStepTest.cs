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
    public class DecayPacketsStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayPacketsStepTest));

        [Test]
        public void DecayTest()
        {
            var game = GameTest.GetSingleUnitGame();
            var player1 = game.Players[0];
            var packet = new MineralPacket()
            {
                Player = player1,
                WarpFactor = 7,
                SafeWarpSpeed = 7,
                DistanceTravelled = 7 * 7
            };
            game.MineralPackets.Add(packet);

            DecayPacketsStep step = new DecayPacketsStep(game);

            // no decay for safe speed
            packet.Cargo = new Cargo(100, 100, 100);
            step.Decay(packet);
            Assert.AreEqual(new Cargo(100, 100, 100), packet.Cargo);

            // 10% decay for 1 over safe speed
            packet.Cargo = new Cargo(100, 100, 100);
            packet.WarpFactor = 8;
            packet.DistanceTravelled = packet.WarpFactor * packet.WarpFactor;
            step.Decay(packet);
            Assert.AreEqual(new Cargo(90, 90, 90), packet.Cargo);

            // 50% decay for 3 over safe speed
            packet.Cargo = new Cargo(100, 100, 100);
            packet.WarpFactor = 10;
            packet.DistanceTravelled = packet.WarpFactor * packet.WarpFactor;
            step.Decay(packet);
            Assert.AreEqual(new Cargo(50, 50, 50), packet.Cargo);


            // half of 50% decay for PP races
            player1.Race.PRT = PRT.PP;
            packet.Cargo = new Cargo(100, 100, 100);
            packet.WarpFactor = 10;
            packet.DistanceTravelled = packet.WarpFactor * packet.WarpFactor;
            step.Decay(packet);
            Assert.AreEqual(new Cargo(75, 75, 75), packet.Cargo);

            // protate distance, if we only travelled half the distance 
            // this year, our 50% packet decay is only 25%
            player1.Race.PRT = PRT.JoaT;
            packet.Cargo = new Cargo(100, 100, 100);
            packet.DistanceTravelled = packet.DistanceTravelled * .5f;
            step.Decay(packet);
            Assert.AreEqual(new Cargo(75, 75, 75), packet.Cargo);


        }
    }
}