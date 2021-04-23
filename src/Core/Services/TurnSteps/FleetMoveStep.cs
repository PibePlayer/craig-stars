using System;
using System.Linq;
using CraigStars.Singletons;
using static CraigStars.Utils.Utils;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetMoveStep : TurnGenerationStep
    {
        MineFieldDamager mineFieldDamager = new MineFieldDamager();
        ShipDesignDiscoverer shipDesignDiscoverer = new ShipDesignDiscoverer();
        public FleetMoveStep(Game game) : base(game, TurnGenerationState.MoveFleets) { }

        public override void Process()
        {
            Game.Fleets.ForEach(fleet => Move(fleet));
        }

        /// <summary>
        /// Move a fleet
        /// </summary>
        /// <param name="fleet"></param>
        internal void Move(Fleet fleet)
        {
            if (fleet.Waypoints.Count > 1)
            {
                Waypoint wp0 = fleet.Waypoints[0];
                Waypoint wp1 = fleet.Waypoints[1];
                float totaldist = wp0.Position.DistanceTo(wp1.Position);
                float dist = wp1.WarpFactor * wp1.WarpFactor;

                // go with the lower
                if (totaldist < dist)
                {
                    dist = totaldist;
                }

                // get the cost for the fleet
                int fuelCost = fleet.GetFuelCost(wp1.WarpFactor, dist);
                int fuelGenerated = 0;
                if (fuelCost > fleet.Fuel)
                {
                    // we will run out of fuel
                    // if this distance would have cost us 10 fuel but we have 6 left, only travel 60% of the distance.
                    var distanceFactor = fleet.Fuel / fuelCost;
                    dist = dist * distanceFactor;

                    // collide with minefields on route, but don't hit a minefield if we run out of fuel beforehand
                    dist = CheckForMineFields(fleet, wp1, dist);

                    fleet.Fuel = 0;
                    wp1.WarpFactor = fleet.GetNoFuelWarpFactor();
                    Message.FleetOutOfFuel(fleet.Player, fleet, wp1.WarpFactor);

                    // if we ran out of fuel 60% of the way to our normal distance, the remaining 40% of our time
                    // was spent travelling at fuel generation speeds:
                    var remainingDistanceTravelled = (1 - distanceFactor) * (wp1.WarpFactor * wp1.WarpFactor);
                    dist += remainingDistanceTravelled;
                    fuelGenerated = fleet.GetFuelGeneration(wp1.WarpFactor, remainingDistanceTravelled);
                }
                else
                {
                    // collide with minefields on route, but don't hit a minefield if we run out of fuel beforehand
                    var actualDist = CheckForMineFields(fleet, wp1, dist);
                    if (actualDist != dist)
                    {
                        dist = actualDist;
                        fuelCost = fleet.GetFuelCost(wp1.WarpFactor, dist);
                        // we hit a minefield, update fuel usage
                    }

                    fleet.Fuel -= fuelCost;
                    fuelGenerated = fleet.GetFuelGeneration(wp1.WarpFactor, dist);
                }

                // message the player about fuel generation
                fuelGenerated = Math.Min(fuelGenerated, fleet.FuelMissing);
                if (fuelGenerated > 0)
                {
                    fleet.Fuel += fuelGenerated;
                    Message.FleetGeneratedFuel(fleet.Player, fleet, fuelGenerated);
                }

                // assuming we move at all, make sure we are no longer orbiting any planets
                if (dist > 0 && fleet.Orbiting != null)
                {
                    fleet.Orbiting.OrbitingFleets.Remove(fleet);
                    fleet.Orbiting = null;
                }

                if (totaldist == dist)
                {
                    fleet.Position = wp1.Position;
                    if (wp1.Target is Planet planet)
                    {
                        fleet.Orbiting = planet;
                        planet.OrbitingFleets.Add(fleet);
                        if (fleet.Player == planet.Player && planet.HasStarbase)
                        {
                            // refuel at starbases
                            fleet.Fuel = fleet.Aggregate.FuelCapacity;
                        }
                    }
                    else if (wp1.Target is Wormhole wormhole)
                    {
                        fleet.Position = wormhole.Destination.Position;
                    }

                    // remove the previous waypoint, it's been processed already
                    if (fleet.RepeatOrders)
                    {
                        var wpToRepeat = fleet.Waypoints[0];
                        wpToRepeat.Target = wpToRepeat.OriginalTarget;
                        wpToRepeat.OriginalPosition = wpToRepeat.OriginalPosition;
                        // if we are supposed to repeat orders, 
                        fleet.Waypoints.Add(wpToRepeat);
                    }
                    fleet.Waypoints.RemoveAt(0);

                    // we arrived, process the current task (the previous waypoint)
                    if (fleet.Waypoints.Count == 1)
                    {
                        fleet.WarpSpeed = 0;
                        fleet.Heading = Vector2.Zero;
                    }
                    else
                    {
                        wp1 = fleet.Waypoints[1];
                        fleet.WarpSpeed = wp1.WarpFactor;
                        fleet.Heading = (wp1.Position - fleet.Position).Normalized();
                    }
                }
                else
                {
                    // move this fleet closer to the next waypoint
                    fleet.WarpSpeed = wp1.WarpFactor;
                    fleet.Heading = (wp1.Position - fleet.Position).Normalized();
                    if (wp0.OriginalTarget == null || wp0.OriginalPosition == Vector2.Zero)
                    {
                        wp0.OriginalTarget = wp0.Target;
                        wp0.OriginalPosition = wp0.Position;
                    }
                    wp0.Target = null;

                    fleet.Position += fleet.Heading * dist;
                    wp0.Position = fleet.Position;
                }
            }
            else
            {
                fleet.WarpSpeed = 0;
                fleet.Heading = Vector2.Zero;
            }
        }

        /// <summary>
        /// Check for mine field collisions. If we collide with one, do damage and stop the fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="dest"></param>
        /// <param name="distance"></param>
        /// <returns>The actual distance travelled, if stopped by a minefield</returns>
        internal float CheckForMineFields(Fleet fleet, Waypoint dest, float distance)
        {
            int safeWarpBonus = 0;
            if (fleet.Player.Race.PRT == PRT.SD)
            {
                safeWarpBonus = Game.Rules.SDSafeWarpBonus;
            }

            // see if we are colliding with any of these minefields
            foreach (var mineField in Game.MineFields.Where(mf => mf.Player != fleet.Player))
            {
                // we only check if we are going faster than allowed by the minefield.
                var stats = Game.Rules.MineFieldStatsByType[mineField.Type];
                if (dest.WarpFactor > stats.MaxSpeed + safeWarpBonus)
                {
                    // this is not our minefield, and we are going fast, check if we intersect.
                    Vector2 from = fleet.Position;
                    Vector2 to = (dest.Position - fleet.Position).Normalized() * distance + from;
                    float collision = SegmentIntersectsCircle(from, to, mineField.Position, mineField.Radius);
                    if (collision == -1)
                    {
                        // miss! phew, that was close!
                        return distance;
                    }
                    else
                    {
                        // we are travelling through this minefield, for each light year we go through, check for a hit
                        // collision is 0 to 1, which is the percent of our travel segment that is NOT in the field.
                        // figure out what that is in lightYears
                        // if we are travelling 32 light years and 3/4 of it is through the minefield, we need to check
                        // for collision 24 times
                        int lightYearsInField = (int)Math.Min(mineField.Radius, Math.Ceiling((1 - collision) * distance));
                        float lightYearsBeforeField = collision * distance;

                        // Each type of minefield has a chance to hit based on how fast
                        // the fleet is travelling through the field. A normal mine has a .3% chance
                        // of hitting a ship per extra warp over warp 4, so a warp 9 ship
                        // has a 1.5% chance of hitting a mine per lightyear travelled
                        int unsafeWarp = dest.WarpFactor - (stats.MaxSpeed + safeWarpBonus);
                        float chanceToHit = stats.ChanceOfHit * unsafeWarp;
                        for (int checkNum = 0; checkNum < lightYearsInField; checkNum++)
                        {
                            if (chanceToHit >= Game.Rules.Random.NextDouble())
                            {
                                // ouch, we hit a minefield!
                                // we stop moving at the hit, so if we made it 8 checks out of 24 for our above example
                                // we only travel 8 lightyears through the field (plus whatever distance we travelled to get to the field)
                                var actualDistanceTravelled = lightYearsBeforeField + checkNum;
                                mineFieldDamager.TakeMineFieldDamage(fleet, mineField, stats);
                                mineFieldDamager.ReduceMineFieldOnImpact(mineField);
                                if (mineField.Player.Race.PRT == PRT.SD)
                                {
                                    // discover any fleets that 
                                    foreach (var token in fleet.Tokens)
                                    {
                                        shipDesignDiscoverer.Discover(mineField.Player, token.Design, true);
                                    }
                                }
                                return actualDistanceTravelled;
                            }
                        }
                    }
                }
            }

            return distance;

        }


    }
}