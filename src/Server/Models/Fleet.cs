using Godot;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Singletons;
using System;

namespace CraigStars
{
    public class Fleet : MapObject
    {
        #region Planet Stats

        public Cargo Cargo { get; } = new Cargo();
        public Planet Orbiting { get; set; }

        public List<Waypoint> Waypoints { get; } = new List<Waypoint>();
        public List<ShipToken> Tokens { get; } = new List<ShipToken>();

        public FleetAggregate Aggregate { get; } = new FleetAggregate();

        #endregion


        public void Move()
        {
            if (Waypoints.Count > 1)
            {
                Waypoint wp0 = Waypoints[0];
                Waypoint wp1 = Waypoints[1];
                float totaldist = wp0.Position.DistanceTo(wp1.Position);
                float dist = wp1.WarpFactor * wp1.WarpFactor;

                // go with the lower
                if (totaldist < dist)
                {
                    dist = totaldist;
                }

                // get the cost for the fleet
                int fuelCost = GetFuelCost(wp1.WarpFactor, dist);
                Cargo.Fuel -= fuelCost;

                // assuming we move at all, make sure we are no longer orbiting any planets
                if (dist > 0 && Orbiting != null)
                {
                    Orbiting.OrbitingFleets.Remove(this);
                    Orbiting = null;
                }

                if (totaldist == dist)
                {
                    Position = wp1.Position;
                    if (wp1.Target != null && wp1.Target is Planet planet)
                    {
                        Orbiting = planet;
                        planet.OrbitingFleets.Add(this);
                    }

                    // remove the previous waypoint
                    Waypoints.RemoveAt(0);

                    // TODO: we've arrived, process this waypoint
                    /*
                    processTask(fleet, wp1);
                    Waypoints.remove(0);
                    if (fleet.getWaypoints().size() == 1) {
                        Message.fleetCompletedAssignedOrders(fleet.getOwner(), fleet);
                    }
                    */
                }
                else
                {
                    // move this fleet closer to the next waypoint
                    var direction = (wp1.Position - Position).Normalized();
                    wp0.Target = null;
                    // sprite.LookAt(wp1.Position);

                    Position += direction * dist;
                    wp0.Position = Position;
                }
            }
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
            // 1 mg of fuel will move 200 kt of weight 1 LY at a Fuel Usage Number of 100.
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
            foreach (var stack in Tokens)
            {
                // figure out this ship stack's mass as well as it's proportion of the cargo
                int mass = stack.Design.Aggregate.Mass * stack.Quantity;
                int fleetCargo = Cargo.Total;
                int stackCapacity = stack.Design.Aggregate.CargoCapacity * stack.Quantity;
                int fleetCapacity = Aggregate.CargoCapacity;

                if (fleetCapacity > 0)
                {
                    mass += (int)((float)fleetCargo * ((float)stackCapacity / (float)fleetCapacity));
                }
                fuelCost += GetFuelCost(warpFactor, mass, distance, ifeFactor, stack.Design.Aggregate.Engine);
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
            // TODO:
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

        public void ComputeAggregate()
        {
            Aggregate.Mass = 0;
            Aggregate.Shield = 0;
            Aggregate.CargoCapacity = 0;
            Aggregate.FuelCapacity = 0;
            Aggregate.Colonizer = false;
            Aggregate.Cost = new Cost();
            Aggregate.SpaceDock = 0;
            Aggregate.ScanRange = 0;
            Aggregate.ScanRangePen = 0;
            Aggregate.Engine = null;

            // compute each token's 
            Tokens.ForEach(token =>
            {
                token.Design.ComputeAggregate(Player);

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

                // colonization
                if (token.Design.Aggregate.Colonizer)
                {
                    Aggregate.Colonizer = true;
                }

                // We should only have one ship stack with spacdock capabilities, but for this logic just go with the max
                Aggregate.SpaceDock = Math.Max(Aggregate.SpaceDock, token.Design.Aggregate.SpaceDock);

                Aggregate.ScanRange = Math.Max(Aggregate.ScanRange, token.Design.Aggregate.ScanRange);
                Aggregate.ScanRangePen = Math.Max(Aggregate.ScanRangePen, token.Design.Aggregate.ScanRangePen);

            });


        }

        /// <summary>
        /// Update a player's copy of this fleet
        /// </summary>
        /// <param name="fleet"></param>
        public void UpdatePlayerFleet(Fleet fleet)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update the report for this fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="penScanned">True if we penscanned it</param>
        public void UpdateReport(Fleet fleet, bool penScanned)
        {
            Position = fleet.Position;

        }
    }
}
