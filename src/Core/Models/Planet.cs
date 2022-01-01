using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using log4net;
using Newtonsoft.Json;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Planet : MapObject, SerializableMapObject, ICargoHolder
    {
        static CSLog log = LogProvider.GetLogger(typeof(Planet));

        #region Scannable Stats

        public Hab? Hab { get; set; }
        public Hab? BaseHab { get; set; }
        public Hab? TerraformedAmount { get; set; }
        public Mineral MineralConcentration { get; set; }

        [JsonIgnore]
        public int Population
        {
            get => Cargo.Colonists * 100;
            set
            {
                Cargo = Cargo.WithColonists(value / 100);
            }
        }

        public Starbase Starbase { get; set; }
        public int PacketSpeed { get; set; } = 0;

        [JsonIgnore]
        public MapObject PacketTarget
        {
            get => packetTarget;
            set
            {
                packetTarget = value;
                if (packetTarget != null)
                {
                    PacketTargetGuid = packetTarget.Guid;
                }
                else
                {
                    PacketTargetGuid = null;
                }
            }
        }
        MapObject packetTarget;
        public Guid? PacketTargetGuid { get; set; }

        [JsonIgnore]
        public MapObject RouteTarget
        {
            get => routeTarget;
            set
            {
                routeTarget = value;
                if (routeTarget != null)
                {
                    RouteTargetGuid = routeTarget.Guid;
                }
                else
                {
                    RouteTargetGuid = null;
                }
            }
        }
        MapObject routeTarget;
        public Guid? RouteTargetGuid { get; set; }

        [JsonIgnore] public bool HasStarbase { get => Starbase != null; }
        [JsonIgnore] public bool HasMassDriver { get => Starbase != null && Starbase.Spec.HasMassDriver; }
        [JsonIgnore] public bool HasStargate { get => Starbase != null && Starbase.Spec.HasStargate; }

        #endregion

        #region Planet Makeup

        public Mineral MineYears { get; set; }
        public Cargo Cargo { get; set; }

        [JsonIgnore]
        public int AvailableCapacity { get => int.MaxValue; }

        [JsonIgnore]
        public int Fuel
        {
            get => Starbase != null ? MapObject.Infinite : 0;
            set
            {
                // ignore setting fuel on a planet
            }
        }

        [JsonIgnore]
        public int FuelCapacity { get => Starbase != null ? MapObject.Infinite : 0; }

        public ProductionQueue ProductionQueue { get; set; }

        public int Mines { get; set; }
        public int Factories { get; set; }
        public int Defenses { get; set; }
        public bool ContributesOnlyLeftoverToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        /// <summary>
        /// If a player with UR scraps a fleet, the planet gets bonus resources that turn
        /// </summary>
        /// <value></value>
        [JsonIgnore] public int BonusResources { get; set; }

        [DefaultValue(Unexplored)]
        public int ReportAge { get; set; } = Unexplored;
        // true if the player has remote mined this planet
        public bool RemoteMined { get; set; } = false;
        public bool Explored { get => ReportAge != Unexplored; }

        /// <summary>
        /// The specs for this planet
        /// </summary>
        /// <returns></returns>
        public PlanetSpec Spec = new();

        #endregion

        /// <summary>
        /// Get the orders for this planet
        /// </summary>
        /// <returns></returns>
        public PlanetProductionOrder GetOrders()
        {
            return new PlanetProductionOrder()
            {
                Guid = Guid,
                Tags = new(Tags),
                ContributesOnlyLeftoverToResearch = ContributesOnlyLeftoverToResearch,
                StarbaseBattlePlanGuid = Starbase?.BattlePlan?.Guid,
                PacketTarget = PacketTarget?.Guid,
                PacketSpeed = PacketSpeed,
                RouteTarget = RouteTarget?.Guid,
                Items = new(ProductionQueue.Items)
            };
        }


        /// <summary>
        /// The client has null values for these, but the server needs to start with
        /// an empty planet
        /// </summary>
        public void InitEmptyPlanet()
        {
            Hab = new Hab();
            MineralConcentration = new Mineral();
            Cargo = new Cargo();
            ProductionQueue = new ProductionQueue();
            MineYears = new Mineral();
        }

        /// <summary>
        /// Transfer cargo to/from the planet. Only allow 
        /// </summary>
        /// <param name="newCargo"></param>
        /// <param name="newFuel"></param>
        /// <returns></returns>
        public CargoTransferResult Transfer(Cargo newCargo, int newFuel = 0)
        {
            // Planets can hold infinite minerals, but we can only transfer away as much as we have
            var transfered = new Cargo(
                Mathf.Clamp(newCargo.Ironium, -Cargo.Ironium, int.MaxValue),
                Mathf.Clamp(newCargo.Boranium, -Cargo.Boranium, int.MaxValue),
                Mathf.Clamp(newCargo.Germanium, -Cargo.Germanium, int.MaxValue),
                Mathf.Clamp(newCargo.Colonists, -Cargo.Colonists, int.MaxValue)
            );

            // transfer the cargo
            Cargo += transfered;

            // can't transfer for fuel to/from a planet, so it's always 0
            return new CargoTransferResult(transfered, fuel: 0);
        }

    }
}