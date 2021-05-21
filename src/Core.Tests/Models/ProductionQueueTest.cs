using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ProductionQueueTest
    {

        [Test]
        public void TestEnsureHasItem()
        {
            var queue = new ProductionQueue();
            var item1 = new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5);

            queue.EnsureHasItem(item1, 0);
            Assert.AreEqual(1, queue.Items.Count);
            Assert.AreEqual(item1, queue.Items[0]);

            // it shouldn't add twice
            item1 = new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5);
            queue.EnsureHasItem(item1, 0);
            Assert.AreEqual(1, queue.Items.Count);
            Assert.AreEqual(item1, queue.Items[0]);

            // Add a second item in spot 2
            var item2 = new ProductionQueueItem(QueueItemType.AutoFactories, 50);
            var item3 = new ProductionQueueItem(QueueItemType.AutoMines, 50);
            queue.EnsureHasItem(item2, 1);
            queue.EnsureHasItem(item3, 2);
            Assert.AreEqual(3, queue.Items.Count);
            Assert.AreEqual(item1, queue.Items[0]);
            Assert.AreEqual(item2, queue.Items[1]);
            Assert.AreEqual(item3, queue.Items[2]);

            // Try adding the first one again, it shouldn't modify the queu
            item1 = new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 5);
            queue.EnsureHasItem(item1, 0);
            queue.EnsureHasItem(item2, 1);
            queue.EnsureHasItem(item3, 2);
            Assert.AreEqual(3, queue.Items.Count);
            Assert.AreEqual(item1, queue.Items[0]);
            Assert.AreEqual(item2, queue.Items[1]);
            Assert.AreEqual(item3, queue.Items[2]);

        }

    }

}