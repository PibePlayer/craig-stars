using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Game logic methods for fleets
    /// </summary>
    public class FleetService
    {

        #region Splits/Merges

        /// <summary>
        /// Merge this fleet with a MergeFleetOrder from the client (or in the UI)
        /// </summary>
        /// <param name="order"></param>
        public void Merge(Fleet fleet, Player player, MergeFleetOrder order)
        {
            // build a dictionary of tokens by design
            var tokenByDesign = fleet.Tokens.ToLookup(token => token.Design).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList()[0]);

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
                        fleet.Tokens.Add(token);
                        tokenByDesign[token.Design] = token;
                    }
                }
                fleet.Cargo += mergedFleet.Cargo;
                fleet.Fuel += mergedFleet.Fuel;

                // remove this merged fleet from our OtherFleets list and the OtherFleets
                // list of every other fleet that considers it an OtherFleet
                fleet.OtherFleets.Remove(mergedFleet);
                mergedFleet.OtherFleets.ForEach(otherFleet => otherFleet.OtherFleets.Remove(mergedFleet));
            }

            fleet.ComputeAggregate(player, recompute: true);
        }

        /// <summary>
        /// Split this fleet into itself and additional fleets
        /// TODO: Figure out damage distribution...
        /// TODO: also, when submitting turns we don't handle changes to fleets after being split...
        /// </summary>
        /// <param name="split"></param>
        /// <returns>The new fleets created by this split command</returns>
        public List<Fleet> Split(Fleet fleet, Player player, SplitAllFleetOrder split)
        {
            List<Fleet> newFleets = new List<Fleet>();
            var originalCargoCapacity = fleet.Aggregate.CargoCapacity;
            var originalFuelCapacity = fleet.Aggregate.FuelCapacity;
            var fuelPercent = (float)fleet.Fuel / fleet.Aggregate.FuelCapacity;

            var count = 0;
            ShipToken remainingToken = null;
            // each new fleet will be assigned the next id
            // available for the player. This is the id
            // we start searching for. This will be updated 
            // for each new fleet that is checked
            var startingId = player.GetNextFleetId();
            foreach (var token in fleet.Tokens)
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
                        var id = startingId++;
                        var newFleet = new Fleet()
                        {
                            Id = id,
                            PlayerNum = player.Num,
                            BaseName = fleet.BaseName,
                            Name = $"{fleet.BaseName} #{id}",
                            Orbiting = fleet.Orbiting,
                            Position = fleet.Position,
                            Tokens = new List<ShipToken>() { new ShipToken() {
                                Design = token.Design,
                                Quantity = 1,
                            }},
                            BattlePlan = fleet.BattlePlan
                        };
                        newFleet.ComputeAggregate(player);
                        newFleet.OtherFleets.AddRange(fleet.OtherFleets);
                        newFleet.OtherFleets.Add(fleet);

                        if (newFleet.Aggregate.CargoCapacity > 0)
                        {
                            // how much of the cargo goes to this token
                            var cargoPercent = (float)newFleet.Aggregate.CargoCapacity / originalCargoCapacity;
                            // copy cargo
                            // TODO: make sure we account for any fractional leftovers
                            newFleet.Cargo = fleet.Cargo * (cargoPercent * newFleet.Aggregate.CargoCapacity);
                        }

                        // copy fuel
                        // TODO: make sure we account for any fractional leftovers
                        newFleet.Fuel = (int)(fuelPercent * newFleet.Aggregate.FuelCapacity);

                        // copy all the waypoints
                        fleet.Waypoints.ForEach(wp =>
                        {
                            newFleet.Waypoints.Add(wp.Clone());
                        });

                        if (fleet.Orbiting != null)
                        {
                            fleet.Orbiting.OrbitingFleets.Add(newFleet);
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
            fleet.Tokens.Clear();
            fleet.Tokens.Add(remainingToken);

            // update our remaining fuel and cargo
            fleet.ComputeAggregate(player, recompute: true);

            // TODO: make sure we account for any fractional leftovers
            if (fleet.Aggregate.CargoCapacity > 0)
            {
                // how much of the cargo goes to this token
                var cargoPercent = (float)fleet.Aggregate.CargoCapacity / originalCargoCapacity;
                fleet.Cargo = fleet.Cargo * cargoPercent;
            }
            fleet.Fuel = (int)(fleet.Aggregate.FuelCapacity * fuelPercent);
            fleet.Name = $"{fleet.BaseName} #{fleet.Id}";
            fleet.OtherFleets.AddRange(newFleets);

            return newFleets;
        }

        #endregion
    
        #region Fuel Usage


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
        public int GetFuelCost(Fleet fleet, Player player, int warpFactor, double distance)
        {
            // figure out how much fuel we're going to use
            double ifeFactor = player.Race.HasLRT(LRT.IFE) ? .85 : 1.0;

            int fuelCost = 0;

            // compute each ship stack separately
            foreach (var token in fleet.Tokens)
            {
                // figure out this ship stack's mass as well as it's proportion of the cargo
                int mass = token.Design.Aggregate.Mass * token.Quantity;
                int fleetCargo = fleet.Cargo.Total;
                int stackCapacity = token.Design.Aggregate.CargoCapacity * token.Quantity;
                int fleetCapacity = fleet.Aggregate.CargoCapacity;

                if (fleetCapacity > 0)
                {
                    mass += (int)((float)fleetCargo * ((float)stackCapacity / (float)fleetCapacity));
                }
                fuelCost += GetFuelCost(warpFactor, mass, distance, ifeFactor, token.Design.Aggregate.Engine);
            }

            return fuelCost;
        }

        /// <summary>
        /// Get the best warp factor for a given waypoint
        /// </summary>
        /// <returns></returns>
        public int GetBestWarpFactor(Fleet fleet, Player player, Waypoint wp0, Waypoint wp1)
        {
            // if our waypoint is 48 ly away, ideally we want warp 7 to make it in 1 year (7 *7 = 49)
            var distance = wp0.Position.DistanceTo(wp1.Position);
            var idealWarp = Mathf.Clamp((int)Math.Ceiling(Math.Sqrt(distance)), 1, 9);

            var fuelUsage = GetFuelCost(fleet, player, idealWarp, distance);
            while (fuelUsage > fleet.Fuel && idealWarp > 1)
            {
                idealWarp--;
                fuelUsage = GetFuelCost(fleet, player, idealWarp, distance);
            }

            return idealWarp;
        }


        /// <summary>
        /// Get the default warp factor of this fleet.
        /// i.e. the highest warp you can travel using only 100% normal fuel
        /// </summary>
        /// <returns></returns>
        public int GetDefaultWarpFactor(Fleet fleet, Player player)
        {
            var warpFactor = 5;
            if (fleet.Aggregate.Engine != null)
            {
                return fleet.Aggregate.Engine.IdealSpeed;
            }
            return warpFactor;
        }

        /// <summary>
        /// Get the warp factor for when we run out of fuel.
        /// </summary>
        /// <returns></returns>
        public int GetNoFuelWarpFactor(Fleet fleet, Player player)
        {
            // find the lowest freeSpeed from all the fleet's engines
            var freeSpeed = int.MaxValue;
            foreach (var token in fleet.Tokens)
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
        public int GetFuelGeneration(Fleet fleet, Player player, int warpFactor, double distance)
        {
            var fuelGenerated = 0.0;
            foreach (var token in fleet.Tokens)
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
        public int GetEstimatedRange(Fleet fleet, Player player)
        {
            // get the cost to go 1000 lightyears and figure out how many times our fuel will do that
            return (int)((float)fleet.Fuel / (float)GetFuelCost(fleet, player, GetDefaultWarpFactor(fleet, player), 1000) * 1000.0f);
        }


        #endregion
 
        #region Battles

        /// <summary>
        /// Return true if this fleet's BattleOrders.AttackWho would attack the owner of the otherFleet
        /// </summary>
        /// <param name="otherFleet"></param>
        /// <returns></returns>
        public bool WillAttack(Fleet fleet, Player player, int otherPlayerNum)
        {
            bool willAttack = false;
            // if we have weapons and we don't own this other fleet, see if we
            // would target it
            if (fleet.Aggregate.HasWeapons && fleet.BattlePlan.Tactic != BattleTactic.Disengage && otherPlayerNum != player.Num)
            {
                switch (fleet.BattlePlan.AttackWho)
                {
                    case BattleAttackWho.Enemies:
                        willAttack = player.IsEnemy(otherPlayerNum);
                        break;
                    case BattleAttackWho.EnemiesAndNeutrals:
                        willAttack = player.IsEnemy(otherPlayerNum) || player.IsNeutral(otherPlayerNum);
                        break;
                    case BattleAttackWho.Everyone:
                        willAttack = true;
                        break;
                }
            }
            return willAttack;
        }

        #endregion

    }
}