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

        public Cost GetCostOfOne(UniverseSettings settings, Race race)
        {
            Cost cost = new Cost();
            if (Type == QueueItemType.Mine || Type == QueueItemType.AutoMine)
            {
                cost.Resources = race.MineCost;
            }
            else if (Type == QueueItemType.Factory || Type == QueueItemType.AutoFactory)
            {
                cost.Resources = race.FactoryCost;
                cost.Germanium = settings.FactoryCostGermanium;
                if (race.FactoriesCostLess)
                {
                    cost.Germanium = cost.Germanium - 1;
                }
            }
            else if (Type == QueueItemType.Defense || Type == QueueItemType.AutoDefense)
            {
                cost = settings.DefenseCost;
            }
            else if (Type == QueueItemType.Alchemy || Type == QueueItemType.AutoAlchemy)
            {
                if (race.HasLRT(LRT.MA))
                {
                    cost.Resources = settings.MineralAlchemyLRTCost;
                }
                else
                {
                    cost.Resources = settings.MineralAlchemyCost;
                }
            }
            else if (Type == QueueItemType.ShipToken || Type == QueueItemType.Starbase)
            {
                cost = Design.Aggregate.Cost;
            }

            return cost;
        }

    }
}