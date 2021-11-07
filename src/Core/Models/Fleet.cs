using System.Collections.Generic;
using System.Linq;
using System;
using log4net;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Godot;

namespace CraigStars
{
    interface IAggregatable<T> where T : ShipDesignAggregate
    {
        T Aggregate { get; }
    }

    [JsonObject(IsReference = true)]
    public class Fleet : MapObject, SerializableMapObject, ICargoHolder, IAggregatable<FleetAggregate>
    {
        static CSLog log = LogProvider.GetLogger(typeof(Fleet));

        /// <summary>
        /// Fleets are named "Long Range Scout #9"
        /// This is the base name without the id number
        /// </summary>
        /// <value></value>
        public string BaseName { get; set; }

        #region Stats

        public int Age { get; set; }
        public Cargo Cargo { get; set; } = new Cargo();
        [JsonIgnore]
        public int AvailableCapacity { get => Aggregate.CargoCapacity - Cargo.Total; }

        public int Fuel { get; set; }
        [JsonIgnore] public int FuelCapacity { get => Aggregate.FuelCapacity; }
        [JsonIgnore] public int FuelMissing { get => Aggregate.FuelCapacity - Fuel; }
        public int Damage { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet Orbiting { get; set; }
        [JsonIgnore]
        public List<Fleet> OtherFleets { get; set; } = new List<Fleet>();

        public bool Scrapped { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
        public bool RepeatOrders { get; set; }

        [JsonProperty(IsReference = true)]
        public BattlePlan BattlePlan { get; set; }

        [JsonProperty(IsReference = true)]
        public FleetComposition FleetComposition { get; set; } = new FleetComposition();

        // These are all publicly viewable when a fleet is scanned
        public List<ShipToken> Tokens { get; set; } = new List<ShipToken>();
        public Vector2 Heading { get; set; }
        public int WarpSpeed { get; set; }
        public int IdleTurns { get; set; }

        /// <summary>
        /// During movement, we keep track of the starting position of each fleet so we can 
        /// pen scan planets as we travel through
        /// </summary>
        /// <value></value>
        [JsonIgnore] public Vector2? PreviousPosition { get; set; }

        [JsonIgnore]
        public int Mass { get => Aggregate.Mass; set => Aggregate.Mass = value; }

        [JsonIgnore] public FleetAggregate Aggregate { get; } = new FleetAggregate();

        public Fleet()
        {
            EventManager.PlayerResearchLevelIncreasedEvent += OnPlayerResearchLevelIncreased;
        }

        ~Fleet()
        {
            EventManager.PlayerResearchLevelIncreasedEvent -= OnPlayerResearchLevelIncreased;
        }

        #endregion

        #region Serializer callbacks

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Orbiting?.OrbitingFleets.Add(this);
        }

        #endregion

        /// <summary>
        /// Get the primary shipToken, i.e. the one that is most powerful
        /// </summary>
        /// <returns></returns>
        public ShipToken GetPrimaryToken()
        {
            // TODO: write this code
            return Tokens.FirstOrDefault();
        }

        public virtual void ComputeAggregate(Player player, bool recompute = false)
        {
            if (Aggregate.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }

            Aggregate.Purposes.Clear();
            Aggregate.MassEmpty = 0;
            Aggregate.Mass = Cargo.Total;
            Aggregate.Shield = 0;
            Aggregate.CargoCapacity = 0;
            Aggregate.FuelCapacity = 0;
            Aggregate.Colonizer = false;
            Aggregate.Cost = new Cost();
            Aggregate.SpaceDock = 0;
            Aggregate.ScanRange = TechHullComponent.NoScanner;
            Aggregate.ScanRangePen = TechHullComponent.NoScanner;
            Aggregate.Engine = null;
            Aggregate.MineSweep = 0;
            Aggregate.MiningRate = 0;

            // Some races cloak cargo for free, otherwise
            // cloaking cargo comes at a penalty
            bool freeCargoCloaking = player.FreeCargoCloaking;
            Aggregate.CloakUnits = 0;
            Aggregate.BaseCloakedCargo = 0;
            Aggregate.ReduceCloaking = 0;

            Aggregate.Bomber = false;
            Aggregate.Bombs.Clear();

            Aggregate.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();

            Aggregate.HasWeapons = false;

            Aggregate.TotalShips = 0;

            // compute each token's 
            Tokens.ForEach(token =>
            {
                token.Design.ComputeAggregate(player);

                // update our total ship count
                Aggregate.TotalShips += token.Quantity;

                Aggregate.Purposes.Add(token.Design.Purpose);

                // TODO: which default engine do we use for multiple fleets?
                Aggregate.Engine = token.Design.Aggregate.Engine;
                // cost
                Cost cost = token.Design.Aggregate.Cost * token.Quantity;
                Aggregate.Cost += cost;

                // mass
                Aggregate.Mass += token.Design.Aggregate.Mass * token.Quantity;
                Aggregate.MassEmpty += token.Design.Aggregate.Mass * token.Quantity;

                // armor
                Aggregate.Armor += token.Design.Aggregate.Armor * token.Quantity;

                // shield
                Aggregate.Shield += token.Design.Aggregate.Shield * token.Quantity;

                // cargo
                Aggregate.CargoCapacity += token.Design.Aggregate.CargoCapacity * token.Quantity;

                // fuel
                Aggregate.FuelCapacity += token.Design.Aggregate.FuelCapacity * token.Quantity;

                // minesweep
                Aggregate.MineSweep += token.Design.Aggregate.MineSweep * token.Quantity;

                // remote mining
                Aggregate.MiningRate += token.Design.Aggregate.MiningRate * token.Quantity;

                // colonization
                if (token.Design.Aggregate.Colonizer)
                {
                    Aggregate.Colonizer = true;
                }

                // aggregate all mine layers in the fleet
                if (token.Design.Aggregate.CanLayMines)
                {
                    foreach (var entry in token.Design.Aggregate.MineLayingRateByMineType)
                    {
                        if (!Aggregate.MineLayingRateByMineType.ContainsKey(entry.Key))
                        {
                            Aggregate.MineLayingRateByMineType[entry.Key] = 0;
                        }
                        Aggregate.MineLayingRateByMineType[entry.Key] += token.Design.Aggregate.MineLayingRateByMineType[entry.Key] * token.Quantity;
                    }
                }

                // We should only have one ship stack with spacdock capabilities, but for this logic just go with the max
                Aggregate.SpaceDock = Math.Max(Aggregate.SpaceDock, token.Design.Aggregate.SpaceDock);

                Aggregate.ScanRange = Math.Max(Aggregate.ScanRange, token.Design.Aggregate.ScanRange);
                Aggregate.ScanRangePen = Math.Max(Aggregate.ScanRangePen, token.Design.Aggregate.ScanRangePen);

                // add bombs
                Aggregate.Bomber = token.Design.Aggregate.Bomber ? true : Aggregate.Bomber;
                Aggregate.Bombs.AddRange(token.Design.Aggregate.Bombs);
                Aggregate.SmartBombs.AddRange(token.Design.Aggregate.SmartBombs);

                // check if any tokens have weapons
                // we process weapon slots per stack, so we don't need to aggregate all
                // weapons in a fleet
                Aggregate.HasWeapons = token.Design.Aggregate.HasWeapons ? true : Aggregate.HasWeapons;

                if (token.Design.Aggregate.CloakUnits > 0)
                {
                    // calculate the cloak units for this token based on the design's cloak units (i.e. 70 cloak units / kT for a stealh cloak)
                    Aggregate.CloakUnits += token.Design.Aggregate.CloakUnits;
                }
                else
                {
                    // if this ship doesn't have cloaking, it counts as cargo (except for races with free cargo cloaking)
                    if (!freeCargoCloaking)
                    {
                        Aggregate.BaseCloakedCargo += token.Design.Aggregate.Mass * token.Quantity;
                    }
                }

                // choose the best tachyon detector ship
                Aggregate.ReduceCloaking = Math.Max(Aggregate.ReduceCloaking, token.Design.Aggregate.ReduceCloaking);
            });

            // compute the cloaking based on the cloak units and cargo
            ComputeCloaking(player);

            // cmopute things about the FleetComposition
            ComputeFleetComposition();

            Aggregate.Computed = true;
        }

        /// <summary>
        /// Compute the ship's aggregate cloaking
        /// </summary>
        public void ComputeCloaking(Player player)
        {
            // figure out how much cargo we are cloaking
            var cloakedCargo = Aggregate.BaseCloakedCargo + (player.FreeCargoCloaking ? 0 : Cargo.Total);
            int cloakUnitsWithCargo = (int)Math.Round(Aggregate.CloakUnits * (float)Aggregate.MassEmpty / (Aggregate.MassEmpty + cloakedCargo));
            Aggregate.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(cloakUnitsWithCargo);
        }

        public void ComputeFleetComposition()
        {
            // default is we are complete if we have no fleet composition
            Aggregate.FleetCompositionComplete = true;
            Aggregate.FleetCompositionTokensRequired = new();

            if (FleetComposition != null && FleetComposition.Type != FleetCompositionType.None)
            {
                var fleetCompositionByPurpose = FleetComposition.GetQuantityByPurpose();
                // check if we have the tokens we need
                foreach (var token in Tokens)
                {
                    if (fleetCompositionByPurpose.TryGetValue(token.Design.Purpose, out var fleetCompositionToken))
                    {
                        int quantityRequired = token.Quantity - fleetCompositionToken.Quantity;
                        if (quantityRequired > 0)
                        {
                            // we still need some of this ShipDesignPurpose
                            fleetCompositionByPurpose[token.Design.Purpose].Quantity = quantityRequired;
                        }
                        else
                        {
                            fleetCompositionByPurpose.Remove(token.Design.Purpose);
                        }
                    }
                }

                // add all the remaining FleetCompositionTokens we need
                Aggregate.FleetCompositionTokensRequired.AddRange(fleetCompositionByPurpose.Values);
                Aggregate.FleetCompositionComplete = Aggregate.FleetCompositionTokensRequired.Count == 0;
            }
        }


        /// <summary>
        /// Update aggregates on level advance
        /// </summary>
        /// <param name="player"></param>
        /// <param name="field"></param>
        /// <param name="level"></param>
        void OnPlayerResearchLevelIncreased(Player player, TechField field, int level)
        {
            if (player.Num != PlayerNum) return;

            if (player.Race != null &&
                player.Race.PRT == PRT.JoaT &&
                field == TechField.Electronics &&
                Tokens.Any(token => token.Design.Hull.BuiltInScannerForJoaT))
            {
                // update any fleets with JoaT hulls
                ComputeAggregate(player, recompute: true);
            }
        }

        /// <summary>
        /// Attempt to transfer cargo to/from this fleet
        /// This is used to handle both immediate cargo transfers that the player made in the UI, and by the waypoint tasks
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        public bool AttemptTransfer(Cargo transfer, int fuelTransfer = 0)
        {
            var cargoResult = Cargo + transfer;
            var fuelResult = Fuel + fuelTransfer;

            if (cargoResult >= 0 && cargoResult.Total <= Aggregate.CargoCapacity && fuelResult <= Aggregate.FuelCapacity)
            {
                // The transfer doesn't leave us with 0 minerals, or with so many minerals and fuel that we can't hold it
                Cargo = cargoResult;
                Fuel = fuelResult;
                return true;
            }
            return false;
        }
    }
}
