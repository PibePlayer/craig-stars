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
                fleet.Fuel -= fuelCost;

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
                    }

                    // remove the previous waypoint, it's been processed already
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