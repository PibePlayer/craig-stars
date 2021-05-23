using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This component is used for 
    /// </summary>
    [Tool]
    public class AvailablePlanetProductionQueueItems : PlanetProductionQueueItems
    {

        public override void _Ready()
        {
            base._Ready();

            table.Data.AddColumn("Item");
            table.ResetTable();
        }

        /// <summary>
        /// When a planet is updated, reset our internal Items list to whatever is currently in the planet queue
        /// </summary>
        protected override void OnPlanetUpdated()
        {
            base.OnPlanetUpdated();
            if (Planet != null)
            {
                AddAvailableItems();
            }
        }

        public override void UpdateItems()
        {
            if (Planet != null)
            {
                table.Data.ClearRows();
                AddAvailableItems();
                table.ResetRows();
            }
        }

        /// <summary>
        /// Add any items to the table that this planet can build
        /// </summary>
        void AddAvailableItems()
        {
            // add each design
            if (Planet.HasStarbase && Planet.Starbase.DockCapacity > 0)
            {
                Me.Designs.ForEach(design =>
                {
                    if (Planet.CanBuild(Me, design.Aggregate.Mass))
                    {
                        if (design.Hull.Starbase)
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.Starbase, design: design));
                        }
                        else
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, design: design));
                        }
                    }
                });

                if (Planet.HasMassDriver)
                {
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.IroniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.BoraniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.GermaniumMineralPacket));
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.MixedMineralPacket));
                }
            }

            // add each type of item.
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Factory));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Mine));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Defenses));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.MineralAlchemy));

            if (Planet.CanTerraform())
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.TerraformEnvironment));
            }

            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactories));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMines));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefenses));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralAlchemy));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMaxTerraform));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMinTerraform));
            if (Planet.HasMassDriver)
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralPacket));
            }

            void AddAvailableItem(ProductionQueueItem item)
            {
                Items.Add(item);
                Color color = Colors.White;
                bool italic = item.IsAuto;

                // TODO: figure out skipping builds
                bool skipped = false;

                var cost = item.GetCostOfOne(Me);

                // Get the total cost of this item plus any previous items in the queue
                // and subtract what we have on hand (that will be applied this year)
                Cost remainingCost = cost - availableCost;

                // If we have a bunch of leftover minerals because our planet is full, 0 those out
                remainingCost = new Cost(
                    Math.Max(0, remainingCost.Ironium),
                    Math.Max(0, remainingCost.Boranium),
                    Math.Max(0, remainingCost.Germanium),
                    Math.Max(0, remainingCost.Resources)
                );

                int numYearsToBuild = remainingCost == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCost / yearlyAvailableCost, 1, int.MaxValue));

                // auto is always the normal color
                if (!item.IsAuto)
                {
                    color = GetColor(numYearsToBuild, numYearsToBuild, skipped);
                }

                table.Data.AddRowAdvanced(metadata: item, color: color, italic: italic, item.FullName);
            }
        }

    }
}