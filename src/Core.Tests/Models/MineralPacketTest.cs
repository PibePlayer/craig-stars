using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class MineralPacketTest
    {

        [Test]
        public void TestTransfer()
        {
            var packet = new MineralPacket();
            packet.Cargo = new Cargo(10, 20, 30);

            // can't give a packet cargo
            var result = packet.Transfer(new Cargo(ironium: 25), 10);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: 0), 0), result);
            Assert.AreEqual(new Cargo(10, 20, 30), packet.Cargo);

            // can only take away cargo, but only down to 0
            packet.Cargo = new Cargo(10, 20, 30);
            result = packet.Transfer(new Cargo(ironium: -20, boranium: -10), 0);
            Assert.AreEqual(new CargoTransferResult(new Cargo(ironium: -10, boranium: -10), 0), result);
            Assert.AreEqual(new Cargo(0, 10, 30), packet.Cargo);

        }
    }

}