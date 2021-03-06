using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    public class ProductionQueue
    {
        public Cost Allocated { get; set; } = new Cost();
        /// <summary>
        /// This is the amount of resources leftover after building
        /// These are used for research
        /// </summary>
        /// <value></value>
        public int LeftoverResources { get; set; }

        public List<ProductionQueueItem> Items { get; set; } = new List<ProductionQueueItem>();

        /// <summary>
        /// Ensure this production queue has something in it
        /// </summary>
        /// <param name="item"></param>
        public void EnsureHasItem(ProductionQueueItem item, int index)
        {
            // remove this item if it already exists
            Items.Remove(item);
            // insert this item into the proper place if it doesn't already exist
            if (index < Items.Count && item != Items[index])
            {
                Items.Insert(index, item);
            }

            // if we want this item after our current items, insert it after
            if (index >= Items.Count)
            {
                Items.Add(item);
            }
        }
    }
}