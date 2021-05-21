using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ProductionQueueItemTest
    {
        Rules rules = new Rules(0);

        [Test]
        public void TestEquals()
        {
            var item1 = new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5);
            var item2 = new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5);

            Assert.AreEqual(item1, item2);
        }

    }

}