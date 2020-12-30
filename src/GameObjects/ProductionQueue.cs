using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class ProductionQueue
    {
        public Planet Planet { get; set; }
        public Cost Allocated { get; set; } = new Cost();
        public List<ProductionQueueItem> Items { get; } = new List<ProductionQueueItem>();
    }
}