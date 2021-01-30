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

    }
}