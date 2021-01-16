using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class ProductionQueue
    {
        public Cost Allocated { get; set; } = new Cost();
        public List<ProductionQueueItem> Items { get; } = new List<ProductionQueueItem>();

        /// <summary>
        /// This is the amount of resources leftover after building
        /// These are used for research
        /// </summary>
        /// <value></value>
        public int LeftoverResources { get; set; }

        public void Copy(ProductionQueue productionQueue)
        {
            Allocated = productionQueue.Allocated;
            Items.Clear();
            Items.AddRange(productionQueue.Items);
        }
    }
}