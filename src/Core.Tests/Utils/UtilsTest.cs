using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using static CraigStars.Utils.Utils;

namespace CraigStars.Tests
{
    [TestFixture]
    public class UtilsTest
    {

        [Test]
        public void TestRoundNearest()
        {
            Assert.AreEqual(100, RoundToNearest(51));
            Assert.AreEqual(0, RoundToNearest(49));
            Assert.AreEqual(2500, RoundToNearest(2451.5f));
        }

        [Test]
        public void TestBlah()
        {
            Vector2 blah = new Vector2(1, 2);
            
        }


    }
}