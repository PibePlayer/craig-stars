using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An item in a ProductionQueue
    /// </summary>
    public struct ProductionQueueItem
    {
        public readonly QueueItemType type;
        public int quantity;

        /// <summary>
        /// The name of the fleet to place this item into
        /// </summary>
        /// <value></value>
        public readonly string fleetName;

        /// <summary>
        /// If this is a ship building item, this is the design to build
        /// </summary>
        /// <value></value>
        [JsonProperty(IsReference = true)]
        public ShipDesign Design { get; set; }

        [JsonConstructor]
        public ProductionQueueItem(QueueItemType type, int quantity = 0, ShipDesign design = null, string fleetName = null)
        {
            this.type = type;
            this.quantity = quantity;
            this.Design = design;
            this.fleetName = fleetName;
        }

        public override string ToString()
        {
            return $"{type} {quantity}{(Design != null ? " " + Design.Name : "")}";
        }

        public override bool Equals(object obj)
        {
            if (obj is ProductionQueueItem item)
            {
                return Equals(item);
            }
            return false;
        }

        public bool Equals(ProductionQueueItem other)
        {
            return type == other.type && quantity == other.quantity && Design == other.Design && fleetName == other.fleetName;
        }

        public static bool operator ==(ProductionQueueItem a, ProductionQueueItem b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProductionQueueItem a, ProductionQueueItem b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            var hashCode = type.GetHashCode() ^ quantity.GetHashCode();
            if (fleetName != null)
            {
                hashCode ^= fleetName.GetHashCode();

            }
            if (Design != null)
            {
                hashCode ^= Design.GetHashCode();
            }
            return hashCode;
        }


        public string ShortName
        {
            get
            {
                switch (type)
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
                        return type.ToString();
                }
            }
        }

        public string FullName
        {
            get
            {
                switch (type)
                {
                    case QueueItemType.Starbase:
                    case QueueItemType.ShipToken:
                        return $"{Design.Name} v{Design.Version}";
                    case QueueItemType.AutoAlchemy:
                        return "Alchemy (Auto Build)";
                    case QueueItemType.AutoMine:
                        return "Mine (Auto Build)";
                    case QueueItemType.AutoFactory:
                        return "Factory (Auto Build)";
                    case QueueItemType.AutoDefense:
                        return "Defense (Auto Build)";
                    default:
                        return type.ToString();
                }
            }
        }

        /// <summary>
        /// Get the cost of a single item in this ProductionQueueItem
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public Cost GetCostOfOne(Rules rules, Race race)
        {
            int resources = 0;
            int germanium = 0;
            if (type == QueueItemType.Mine || type == QueueItemType.AutoMine)
            {
                resources = race.MineCost;
            }
            else if (type == QueueItemType.Factory || type == QueueItemType.AutoFactory)
            {
                resources = race.FactoryCost;
                germanium = rules.FactoryCostGermanium;
                if (race.FactoriesCostLess)
                {
                    germanium = germanium - 1;
                }
            }
            else if (type == QueueItemType.Alchemy || type == QueueItemType.AutoAlchemy)
            {
                if (race.HasLRT(LRT.MA))
                {
                    resources = rules.MineralAlchemyLRTCost;
                }
                else
                {
                    resources = rules.MineralAlchemyCost;
                }
            }
            else if (type == QueueItemType.Defense || type == QueueItemType.AutoDefense)
            {
                return rules.DefenseCost;
            }
            else if (type == QueueItemType.ShipToken || type == QueueItemType.Starbase)
            {
                // ship designs have their own cost
                return Design.Aggregate.Cost;
            }

            return new Cost(germanium: germanium, resources: resources);
        }

    }
}