using System;

namespace CraigStars
{
    /// <summary>
    /// An item in a ProductionQueue
    /// </summary>
    public class ProductionQueueItem
    {
        public QueueItemType Type { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// If this is a ship building item, this is the design to build
        /// </summary>
        /// <value></value>
        public ShipDesign Design { get; set; }

        /// <summary>
        /// The name of the fleet to place this item into
        /// </summary>
        /// <value></value>
        public String FleetName { get; set; }

        public String ShortName
        {
            get
            {
                switch (Type)
                {
                    case QueueItemType.Starbase:
                    case QueueItemType.ShipToken:
                        return Design.Name;
                    case QueueItemType.AutoMine:
                        return "Mine (Auto)";
                    case QueueItemType.AutoFactory:
                        return "Factory (Auto)";
                    case QueueItemType.AutoDefense:
                        return "Defenses (Auto)";
                    case QueueItemType.AutoAlchemy:
                        return "Alchemy (Auto)";
                    default:
                        return Type.ToString();
                }
            }
        }

        public String FullName
        {
            get
            {
                switch (Type)
                {
                    case QueueItemType.Starbase:
                    case QueueItemType.ShipToken:
                        return Design.Name;
                    case QueueItemType.AutoAlchemy:
                        return "Alchemy (Auto Build)";
                    case QueueItemType.AutoMine:
                        return "Mine (Auto Build)";
                    case QueueItemType.AutoFactory:
                        return "Factory (Auto Build)";
                    case QueueItemType.AutoDefense:
                        return "Defense (Auto Build)";
                    default:
                        return Type.ToString();
                }
            }
        }

        public ProductionQueueItem(QueueItemType type, int quantity = 1, ShipDesign design = null)
        {
            Type = type;
            Quantity = quantity;
            Design = design;
        }

        public ProductionQueueItem Clone()
        {
            return new ProductionQueueItem(Type, Quantity, Design);
        }

        /// <summary>
        /// Get the cost of a single item in this ProductionQueueItem
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public Cost GetCostOfOne(UniverseSettings settings, Race race)
        {
            int resources = 0;
            int germanium = 0;
            if (Type == QueueItemType.Mine || Type == QueueItemType.AutoMine)
            {
                resources = race.MineCost;
            }
            else if (Type == QueueItemType.Factory || Type == QueueItemType.AutoFactory)
            {
                resources = race.FactoryCost;
                germanium = settings.FactoryCostGermanium;
                if (race.FactoriesCostLess)
                {
                    germanium = germanium - 1;
                }
            }
            else if (Type == QueueItemType.Alchemy || Type == QueueItemType.AutoAlchemy)
            {
                if (race.HasLRT(LRT.MA))
                {
                    resources = settings.MineralAlchemyLRTCost;
                }
                else
                {
                    resources = settings.MineralAlchemyCost;
                }
            }
            else if (Type == QueueItemType.Defense || Type == QueueItemType.AutoDefense)
            {
                return settings.DefenseCost;
            }
            else if (Type == QueueItemType.ShipToken || Type == QueueItemType.Starbase)
            {
                // ship designs have their own cost
                return Design.Aggregate.Cost;
            }

            return new Cost(germanium: germanium, resources: resources);
        }

    }
}