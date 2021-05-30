using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
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
        /// <param name="index">Set the index to insert this item, or -1 to append it to the end</param>
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

            if (index == -1 || index >= Items.Count)
            {
                // if we want this item after our current items, insert it after
                Items.Add(item);
            }
            else if (index < Items.Count && item != Items[index])
            {
                // insert this item into the proper place if it doesn't already exist
                Items.Insert(index, item);
            }

        }

        /// <summary>
        /// Remove a production queue item by type (and ship design, if specified)
        /// </summary>
        /// <param name="MaxTerraform"></param>
        public void RemoveItem(QueueItemType type, ShipDesign design = null)
        {
            Items.RemoveAll(item => item.Type == type && item.Design == design);
        }

        /// <summary>
        /// Add this item after the first instance of give production queue item type
        /// Note: if there are more than one type (like an auto minex10 and later an auto minex50, 
        /// this will go after the auto minex10)
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="afterType">The type to add this item after</param>
        public void AddAfter(ProductionQueueItem item, QueueItemType afterType)
        {
            for (int index = 0; index < Items.Count; index++)
            {
                if (Items[index].Type == afterType)
                {
                    // add it after this items
                    Items.Insert(index + 1, item);
                    break;
                }
            }

        }
    }
}