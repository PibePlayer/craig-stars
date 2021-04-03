using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class CostTest
    {

        [Test]
        public void TestDivide()
        {
            Cost cost = new Cost(1, 2, 3, 4);
            Assert.AreEqual(1, cost / cost);
            Assert.AreEqual(2, new Cost(2, 4, 6, 8) / cost);
            Assert.AreEqual(.5f, cost / new Cost(2, 4, 6, 8));

            // Our item costs 8 reosources and we have loads of minerals, but only 
            // 4 resources available, we can build half of this item
            Cost available = new Cost(100, 200, 300, 4);
            cost = new Cost(0, 0, 0, 8);
            Assert.AreEqual(.5f, available / cost);
        }

    }

}