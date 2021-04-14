using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetMoveStep : Step
    {
        public FleetMoveStep(Game game, TurnGeneratorState state) : base(game, state) { }

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
                    fleet.Fuel -= fuelCost;
                    fuelGenerated = fleet.GetFuelGeneration(wp1.WarpFactor, dist);

                }

                // message the player about fuel generation
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
                    if (wp1.Target != null && wp1.Target is Planet planet)
                    {
                        fleet.Orbiting = planet;
                        planet.OrbitingFleets.Add(fleet);
                        if (fleet.Player == planet.Player && planet.HasStarbase)
                        {
                            // refuel at starbases
                            fleet.Fuel = fleet.Aggregate.FuelCapacity;
                        }
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
    }
}