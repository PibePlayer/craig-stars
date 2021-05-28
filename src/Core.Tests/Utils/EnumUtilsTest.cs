using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Utils;

namespace CraigStars.Tests
{
    [TestFixture]
    public class EnumUtilsTest
    {
        [Test]
        public void TestGetLabelForMineFieldType()
        {
            Assert.AreEqual("Speed Bump", EnumUtils.GetLabelForMineFieldType(MineFieldType.SpeedBump));
            Assert.AreEqual("Heavy", EnumUtils.GetLabelForMineFieldType(MineFieldType.Heavy));
        }
    }
}