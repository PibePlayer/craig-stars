using System;
using Newtonsoft.Json;

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
        /// The name of the fleet to place this item into
        /// </summary>
        /// <value></value>
        public string FleetName { get; set; }

        /// <summary>
        /// If this is a ship building item, this is the design to build
        /// </summary>
        /// <value></value>
        [JsonProperty(IsReference = true)]
        public ShipDesign Design { get; set; }

        [JsonIgnore]
        public int yearsToBuild { get; set; } = -1;

        [JsonIgnore]
        public float percentComplete { get; set; } = 0;

        [JsonConstructor]
        public ProductionQueueItem(QueueItemType type, int quantity = 0, ShipDesign design = null, string fleetName = null)
        {
            Type = type;
            Quantity = quantity;
            Design = design;
            FleetName = fleetName;
        }

        public ProductionQueueItem Clone()
        {
            return new ProductionQueueItem(Type, Quantity, Design, FleetName);
        }

        public override string ToString()
        {
            return $"{Type} {Quantity}{(Design != null ? " " + Design.Name : "")}";
        }

        public override bool Equals(object obj) => this.Equals(obj as ProductionQueueItem);

        public bool Equals(ProductionQueueItem p)
        {
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (Type == p.Type) && (Quantity == p.Quantity) && (Design == p.Design) && (FleetName == p.FleetName);
        }

        public override int GetHashCode() => (Type, Quantity, Design, FleetName).GetHashCode();

        public static bool operator ==(ProductionQueueItem lhs, ProductionQueueItem rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ProductionQueueItem lhs, ProductionQueueItem rhs) => !(lhs == rhs);


        public string ShortName
        {
            get
            {
                switch (Type)
                {
                    case QueueItemType.Starbase:
                    case QueueItemType.ShipToken:
                        return Design.Name;
                    case QueueItemType.AutoMines:
                        return "Mine (Auto)";
                    case QueueItemType.AutoFactories:
                        return "Factory (Auto)";
                    case QueueItemType.AutoDefenses:
                        return "Defenses (Auto)";
                    case QueueItemType.AutoMineralAlchemy:
                        return "Alchemy (Auto)";
                    default:
                        return Type.ToString();
                }
            }
        }

        public string FullName
        {
            get
            {
                switch (Type)
                {
                    case QueueItemType.Starbase:
                    case QueueItemType.ShipToken:
                        return $"{Design.Name} v{Design.Version}";
                    case QueueItemType.AutoMineralAlchemy:
                        return "Alchemy (Auto Build)";
                    case QueueItemType.AutoMines:
                        return "Mine (Auto Build)";
                    case QueueItemType.AutoFactories:
                        return "Factory (Auto Build)";
                    case QueueItemType.AutoDefenses:
                        return "Defense (Auto Build)";
                    default:
                        return Type.ToString();
                }
            }
        }

        /// <summary>
        /// True if this is a mineral packet
        /// </summary>
        /// <value></value>
        public bool IsPacket
        {
            get =>
                Type == QueueItemType.IroniumMineralPacket ||
                Type == QueueItemType.BoraniumMineralPacket ||
                Type == QueueItemType.GermaniumMineralPacket ||
                Type == QueueItemType.MixedMineralPacket ||
                Type == QueueItemType.AutoMineralPacket;
        }


        /// <summary>
        /// True if this is a mineral packet
        /// </summary>
        /// <value></value>
        public bool IsAuto
        {
            get =>
                Type == QueueItemType.AutoDefenses ||
                Type == QueueItemType.AutoFactories ||
                Type == QueueItemType.AutoMaxTerraform ||
                Type == QueueItemType.AutoMineralAlchemy ||
                Type == QueueItemType.AutoMineralPacket ||
                Type == QueueItemType.AutoMines ||
                Type == QueueItemType.AutoMinTerraform;
        }

        /// <summary>
        /// Get the cost of a single item in this ProductionQueueItem
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public Cost GetCostOfOne(Rules rules, Player player)
        {
            var race = player.Race;
            int resources = 0;
            int germanium = 0;
            if (Type == QueueItemType.Mine || Type == QueueItemType.AutoMines)
            {
                resources = race.MineCost;
            }
            else if (Type == QueueItemType.Factory || Type == QueueItemType.AutoFactories)
            {
                resources = race.FactoryCost;
                germanium = rules.FactoryCostGermanium;
                if (race.FactoriesCostLess)
                {
                    germanium = germanium - 1;
                }
            }
            else if (Type == QueueItemType.MineralAlchemy || Type == QueueItemType.AutoMineralAlchemy)
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
            else if (Type == QueueItemType.Defenses || Type == QueueItemType.AutoDefenses)
            {
                return rules.DefenseCost;
            }
            else if (Type == QueueItemType.IroniumMineralPacket)
            {
                if (race.PRT == PRT.PP)
                {
                    return new Cost(ironium: rules.MineralsPerSingleMineralPacketPP, resources: rules.PacketResourceCostPP);
                }
                else
                {
                    return new Cost(ironium: rules.MineralsPerSingleMineralPacket, resources: rules.PacketResourceCost);
                }
            }
            else if (Type == QueueItemType.BoraniumMineralPacket)
            {
                if (race.PRT == PRT.PP)
                {
                    return new Cost(boranium: rules.MineralsPerSingleMineralPacketPP, resources: rules.PacketResourceCostPP);
                }
                else
                {
                    return new Cost(boranium: rules.MineralsPerSingleMineralPacket, resources: rules.PacketResourceCost);
                }
            }
            else if (Type == QueueItemType.GermaniumMineralPacket)
            {
                if (race.PRT == PRT.PP)
                {
                    return new Cost(germanium: rules.MineralsPerSingleMineralPacketPP, resources: rules.PacketResourceCostPP);
                }
                else
                {
                    return new Cost(germanium: rules.MineralsPerSingleMineralPacket, resources: rules.PacketResourceCost);
                }
            }
            else if (Type == QueueItemType.MixedMineralPacket || Type == QueueItemType.AutoMineralPacket)
            {
                if (race.PRT == PRT.PP)
                {
                    return new Cost(rules.MineralsPerMixedMineralPacketPP, rules.MineralsPerMixedMineralPacketPP, rules.MineralsPerMixedMineralPacketPP, resources: rules.PacketResourceCostPP);
                }
                else
                {
                    return new Cost(rules.MineralsPerMixedMineralPacket, rules.MineralsPerMixedMineralPacket, rules.MineralsPerMixedMineralPacket, resources: rules.PacketResourceCost);
                }
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