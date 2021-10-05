using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Fleet's have battle orders assigned made up of 4 parts
    /// * Primary target type
    /// * Secondary target type
    /// * Tactic
    /// * Which players the fleet should attack
    /// </summary>
    public class ProductionPlan : PlayerPlan<ProductionPlan>
    {
        public ProductionPlan() : base() { }
        public ProductionPlan(string name) : base(name) { }

        public List<ProductionQueueItem> Items { get; set; } = new List<ProductionQueueItem>();
        public bool ContributesOnlyLeftoverToResearch { get; set; }

        /// <summary>
        /// Make a clone of this battle plan
        /// </summary>
        /// <returns></returns>
        public override ProductionPlan Clone()
        {
            return new ProductionPlan()
            {
                Guid = Guid,
                Name = Name,
                Items = Items.Select(item => item.Clone()).ToList(),
                ContributesOnlyLeftoverToResearch = ContributesOnlyLeftoverToResearch
            };
        }
    }
}