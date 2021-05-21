using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CraigStars
{
    public class ProductionQueue
    {
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
        public void EnsureHasItem(ProductionQueueItem item, int index = -1)
        {
            foreach (var existingItem in Items)
            {
                if (item.Type == existingItem.Type && item.Quantity == existingItem.Quantity)
                {
                    // don't add it if it exists somewhere else in the queue
                    return;
                }
            }

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