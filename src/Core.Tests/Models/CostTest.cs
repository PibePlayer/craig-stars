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
            cost = new Cost(0, 0, 0, 8);
            Cost available = new Cost(100, 200, 300, 4);
            Assert.AreEqual(.5f, available / cost);

            // one factory, no resources
            available = new Cost(10, 10, 10, 0);
            cost = new Cost(0, 0, 4, 10);
            Assert.AreEqual(0, available / cost);

            // we have 21 factories to build and only 30 resources (and some minerals) a turn
            // it should take us 7 years to build it
            Cost remainingCostOfAll = new Cost(0, 0, 0, 210);
            Cost yearlyAvailableCost = new Cost(8, 3, 10, 30);
            Assert.AreEqual(7, (int)(1f / (yearlyAvailableCost / remainingCostOfAll) + .5f));

        }

    }

}