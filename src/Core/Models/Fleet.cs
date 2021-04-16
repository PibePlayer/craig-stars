using System.Collections.Generic;
using System.Linq;
using System;
using log4net;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Godot;
using CraigStars.Singletons;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Fleet : MapObject, SerializableMapObject, ICargoHolder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Fleet));

        #region Stats

        public Cargo Cargo { get; set; } = new Cargo();
        [JsonIgnore]
        public int AvailableCapacity { get => Aggregate.CargoCapacity - Cargo.Total; }

        public int Fuel { get; set; }
        public int Damage { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet Orbiting { get; set; }
        [JsonIgnore]
        public List<Fleet> OtherFleets { get; set; } = new List<Fleet>();

        public bool Scrapped { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
        public bool RepeatOrders { get; set; }

        [JsonProperty(IsReference = true)]
        public BattlePlan BattlePlan { get; set; } = new BattlePlan();

        // These are all publicly viewable when a fleet is scanned
        public List<ShipToken> Tokens { get; set; } = new List<ShipToken>();
        public Vector2 Heading { get; set; }
        public int WarpSpeed { get; set; }
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



        /// <summary>
        /// Merge this fleet with a MergeFleetOrder from the client (or in the UI)
        /// </summary>
        /// <param name="order"></param>
        public void Merge(MergeFleetOrder order)
        {
            // build a dictionary of tokens by design
            var tokenByDesign = Tokens.ToLookup(token => token.Design).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList()[0]);

            foreach (var mergedFleet in order.MergingFleets)
            {
                foreach (var token in mergedFleet.Tokens)
                {
                    // if we already have this design in our 
                    if (tokenByDesign.TryGetValue(token.Design, out var existingToken))
                    {
                        var originalQuantityDamaged = existingToken.QuantityDamaged;
                        var incomingQuantityDamaged = token.QuantityDamaged;

                        existingToken.Quantity += token.Quantity;

                        // merge in damange 
                        if (incomingQuantityDamaged > 0 || originalQuantityDamaged > 0)
                        {
                            var originalDamage = existingToken.Damage;
                            var incomingDamage = token.Damage;
                            // if we have 5 scouts at 20% damage and 5 scouts at 10% damage
                            // we get 10 scounts at 15% damage
                            existingToken.Damage = (originalDamage * originalQuantityDamaged + incomingDamage * incomingQuantityDamaged) / (originalQuantityDamaged + incomingQuantityDamaged);
                            existingToken.QuantityDamaged = incomingQuantityDamaged + originalQuantityDamaged;
                        }
                    }
                    else
                    {
                        Tokens.Add(token);
                        tokenByDesign[token.Design] = token;
                    }
                }
                Cargo += mergedFleet.Cargo;
                Fuel += mergedFleet.Fuel;

                // remove this merged fleet from our OtherFleets list and the OtherFleets
                // list of every other fleet that considers it an OtherFleet
                OtherFleets.Remove(mergedFleet);
                mergedFleet.OtherFleets.ForEach(otherFleet => otherFleet.OtherFleets.Remove(mergedFleet));
            }

            ComputeAggregate(recompute: true);


        }

        /// <summary>
        /// Split this fleet into itself and additional fleets
        /// TODO: Figure out damage distribution...
        /// TODO: also, when submitting turns we don't handle changes to fleets after being split...
        /// </summary>
        /// <param name="split"></param>
        /// <returns>The new fleets created by this split command</returns>
        public List<Fleet> Split(SplitAllFleetOrder split)
        {
            List<Fleet> newFleets = new List<Fleet>();
            var originalCargoCapacity = Aggregate.CargoCapacity;
            var originalFuelCapacity = Aggregate.FuelCapacity;
            var fuelPercent = (float)Fuel / Aggregate.FuelCapacity;

            var count = 0;
            ShipToken remainingToken = null;
            foreach (var token in Tokens)
            {
                for (int i = 0; i < token.Quantity; i++)
                {
                    if (count == 0)
                    {
                        // the first token becomes our existing fleet
                        remainingToken = token;
                    }
                    else
                    {
                        var newFleet = new Fleet()
                        {
                            Id = Player.GetNextFleetId(),
                            Player = Player,
                            Name = $"{token.Design.Name} #{Player.Stats.NumFleetsBuilt}",
                            Orbiting = Orbiting,
                            Position = Position,
                            Tokens = new List<ShipToken>() { new ShipToken() {
                                Design = token.Design,
                                Quantity = 1,
                            }},
                            BattlePlan = BattlePlan
                        };
                        newFleet.ComputeAggregate();
                        newFleet.OtherFleets.AddRange(OtherFleets);
                        newFleet.OtherFleets.Add(this);

                        if (newFleet.Aggregate.CargoCapacity > 0)
                        {
                            // how much of the cargo goes to this token
                            var cargoPercent = (float)newFleet.Aggregate.CargoCapacity / originalCargoCapacity;
                            // copy cargo
                            // TODO: make sure we account for any fractional leftovers
                            newFleet.Cargo = Cargo * (cargoPercent * newFleet.Aggregate.CargoCapacity);
                        }

                        // copy fuel
                        // TODO: make sure we account for any fractional leftovers
                        newFleet.Fuel = (int)(fuelPercent * newFleet.Aggregate.FuelCapacity);

                        // copy all the waypoints
                        Waypoints.ForEach(wp =>
                        {
                            newFleet.Waypoints.Add(wp.Clone());
                        });

                        if (Orbiting != null)
                        {
                            Orbiting.OrbitingFleets.Add(newFleet);
                        }

                        if (split.NewFleetGuids.Count > i)
                        {
                            newFleet.Guid = split.NewFleetGuids[i];
                        }
                        else
                        {
                            split.NewFleetGuids.Add(newFleet.Guid);
                        }

                        newFleets.Add(newFleet);
                    }
                    count++;
                }
            }

            // this fleet now has one token left
            remainingToken.Quantity = 1;
            Tokens.Clear();
            Tokens.Add(remainingToken);

            // update our remaining fuel and cargo
            ComputeAggregate(recompute: true);

            // TODO: make sure we account for any fractional leftovers
            if (Aggregate.CargoCapacity > 0)
            {
                // how much of the cargo goes to this token
                var cargoPercent = (float)Aggregate.CargoCapacity / originalCargoCapacity;
                Cargo = Cargo * cargoPercent;
            }
            Fuel = (int)(Aggregate.FuelCapacity * fuelPercent);
            Name = $"{remainingToken.Design.Name} #{Id}";
            OtherFleets.AddRange(newFleets);

            return newFleets;
        }

        /// <summary>
        /// Fuel usage calculation courtesy of m.a@stars
        /// </summary>
        /// <param name = "warpFactor" > The warp speed 1 to 10</param>
        /// <param name = "mass" > The mass of the fleet</param>
        /// <param name = "dist" > The distance travelled</param>
        /// <param name = "ifeFactor" > The factor for improved fuel efficiency (.85 if you have the LRT)</param>
        /// <param name = "engine" > The engine being used</param>
        /// <return> The amount of mg of fuel used</return>
        internal int GetFuelCost(int warpFactor, int mass, double dist, double ifeFactor, TechEngine engine)
        {
            if (warpFactor == 0)
            {
                return 0;
            }
            // 1 mg of fuel will move 200kT of weight 1 LY at a Fuel Usage Number of 100.
            // Number of engines doesn't matter. Neither number of ships with the same engine.

            double distanceCeiling = Math.Ceiling(dist); // rounding to next integer gives best graph fit
                                                         // window.status = 'Actual distance used is ' + Distan + 'ly';

            // IFE is applied to drive specifications, just as the helpfile hints.
            // Stars! probably does it outside here once per turn per engine to save time.
            double engineEfficiency = Math.Ceiling(ifeFactor * engine.FuelUsage[warpFactor - 1]);

            // 20000 = 200*100
            // Safe bet is Stars! does all this with integer math tricks.
            // Subtracting 2000 in a loop would be a way to also get the rounding.
            // Or even bitshift for the 2 and adjust "decimal point" for the 1000
            double teorFuel = (Math.Floor(mass * engineEfficiency * distanceCeiling / 2000) / 10);
            // using only one decimal introduces another artifact: .0999 gets rounded down to .0

            // The heavier ships will benefit the most from the accuracy
            int intFuel = (int)Math.Ceiling(teorFuel);

            // That's all. Nothing really fancy, much less random. Subtle differences in
            // math lib workings might explain the rarer and smaller discrepancies observed
            return intFuel;
            // Unrelated to this fuel math are some quirks inside the
            // "negative fuel" watchdog when the remainder of the
            // trip is < 1 ly. Aahh, the joys of rounding! ;o)
        }

        /// <summary>
        /// Get the Fuel cost for this fleet to travel a certain distance at a certain speed
        /// </summary>
        /// <param name="warpFactor"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public int GetFuelCost(int warpFactor, double distance)
        {
            // figure out how much fuel we're going to use
            double ifeFactor = Player.Race.HasLRT(LRT.IFE) ? .85 : 1.0;

            int fuelCost = 0;

            // compute each ship stack separately
            foreach (var token in Tokens)
            {
                // figure out this ship stack's mass as well as it's proportion of the cargo
                int mass = token.Design.Aggregate.Mass * token.Quantity;
                int fleetCargo = Cargo.Total;
                int stackCapacity = token.Design.Aggregate.CargoCapacity * token.Quantity;
                int fleetCapacity = Aggregate.CargoCapacity;

                if (fleetCapacity > 0)
                {
                    mass += (int)((float)fleetCargo * ((float)stackCapacity / (float)fleetCapacity));
                }
                fuelCost += GetFuelCost(warpFactor, mass, distance, ifeFactor, token.Design.Aggregate.Engine);
            }

            return fuelCost;
        }

        /// <summary>
        /// Get the default warp factor of this fleet.
        /// i.e. the highest warp you can travel using only 100% normal fuel
        /// </summary>
        /// <returns></returns>
        public int GetDefaultWarpFactor()
        {
            var warpFactor = 5;
            if (Aggregate.Engine != null)
            {
                var lowestFuelUsage = 0;
                for (int i = 1; i < Aggregate.Engine.FuelUsage.Length; i++)
                {
                    // find the lowest fuel usage until we use more than 100%
                    var fuelUsage = Aggregate.Engine.FuelUsage[i];
                    if (fuelUsage > 100)
                    {
                        break;
                    }
                    else
                    {
                        lowestFuelUsage = fuelUsage;
                        warpFactor = i;
                    }
                }
            }
            return warpFactor;
        }

        /// <summary>
        /// Get the warp factor for when we run out of fuel.
        /// </summary>
        /// <returns></returns>
        public int GetNoFuelWarpFactor()
        {
            // find the lowest freeSpeed from all the fleet's engines
            var freeSpeed = int.MaxValue;
            foreach (var token in Tokens)
            {
                freeSpeed = Math.Min(freeSpeed, token.Design.Aggregate.Engine.FreeSpeed);
            }
            return freeSpeed;
        }

        /// <summary>
        /// Get the amount of fuel this ship will generate at a given warp
        /// F = 0 if the engine is running above the highest warp at which it travels for free (i.e. it is using fuel) 
        /// F = D if the engine is running at the highest warp at which it travels for free 
        /// F = 3D if the engine is running 1 warp factor below the highest warp at which it travels for free 
        /// F = 6D if the engine is running 2 warp factors below the highest warp at which it travels for free 
        /// F = 10D if the engine is running 3 or more warp factors below the highest warp at which it travels for free
        /// Note that the fuel generated is per engine, not per ship; i.e.; a ship with 2, 3, or 4 engines 
        /// produces (or uses) 2, 3, or 4 times as much fuel as a single engine ship.
        /// </summary>
        /// <returns></returns>
        public int GetFuelGeneration(int warpFactor, double distance)
        {
            var fuelGenerated = 0.0;
            foreach (var token in Tokens)
            {
                var freeSpeed = token.Design.Aggregate.Engine.FreeSpeed;
                var numEngines = token.Design.Aggregate.NumEngines;
                var speedDifference = freeSpeed - warpFactor;
                if (speedDifference == 0)
                {
                    fuelGenerated += distance * numEngines;
                }
                else if (speedDifference == 1)
                {
                    fuelGenerated += (3 * distance) * numEngines;
                }
                else if (speedDifference == 2)
                {
                    fuelGenerated += (6 * distance) * numEngines;
                }
                else if (speedDifference >= 3)
                {
                    fuelGenerated += (10 * distance) * numEngines;
                }
            }

            return (int)fuelGenerated;
        }

        /// <summary>
        /// Get the estimated range, in light years, for this fleet going the default warp
        /// </summary>
        /// <returns></returns>
        public int GetEstimatedRange()
        {
            // get the cost to go 1000 lightyears and figure out how many times our fuel will do that
            return (int)((float)Fuel / (float)GetFuelCost(GetDefaultWarpFactor(), 1000) * 1000.0f);
        }

        /// <summary>
        /// Return true if this fleet's BattleOrders.AttackWho would attack the owner of the otherFleet
        /// </summary>
        /// <param name="otherFleet"></param>
        /// <returns></returns>
        public bool WillAttack(PublicPlayerInfo otherPlayer)
        {
            bool willAttack = false;
            // if we have weapons and we don't own this other fleet, see if we
            // would target it
            if (Aggregate.HasWeapons && BattlePlan.Tactic != BattleTactic.Disengage && otherPlayer.Num != Player.Num)
            {
                switch (BattlePlan.AttackWho)
                {
                    case BattleAttackWho.Enemies:
                        willAttack = Player.IsEnemy(otherPlayer);
                        break;
                    case BattleAttackWho.EnemiesAndNeutrals:
                        willAttack = Player.IsEnemy(otherPlayer) || Player.IsNeutral(otherPlayer);
                        break;
                    case BattleAttackWho.Everyone:
                        willAttack = true;
                        break;
                }
            }
            return willAttack;
        }

        public void ComputeAggregate(bool recompute = false)
        {
            if (Aggregate.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }

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
            Aggregate.Purposes.Clear();

            Aggregate.Bomber = false;
            Aggregate.Bombs.Clear();

            Aggregate.HasWeapons = false;

            // compute each token's 
            Tokens.ForEach(token =>
            {
                token.Design.ComputeAggregate(Player);

                Aggregate.Purposes.Add(token.Design.Purpose);

                // TODO: which default engine do we use for multiple fleets?
                Aggregate.Engine = token.Design.Aggregate.Engine;
                // cost
                Cost cost = token.Design.Aggregate.Cost * token.Quantity;
                Aggregate.Cost += cost;

                // mass
                Aggregate.Mass += token.Design.Aggregate.Mass * token.Quantity;

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

                // colonization
                if (token.Design.Aggregate.Colonizer)
                {
                    Aggregate.Colonizer = true;
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
            });

            Aggregate.Computed = true;
        }

        /// <summary>
        /// Update aggregates on level advance
        /// </summary>
        /// <param name="player"></param>
        /// <param name="field"></param>
        /// <param name="level"></param>
        void OnPlayerResearchLevelIncreased(Player player, TechField field, int level)
        {
            if (player != Player) return;

            if (player.Race != null &&
                player.Race.PRT == PRT.JoaT &&
                field == TechField.Electronics &&
                Tokens.Any(token => token.Design.Hull.BuiltInScannerForJoaT))
            {
                // update any fleets with JoaT hulls
                ComputeAggregate(recompute: true);
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
