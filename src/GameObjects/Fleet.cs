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
        public static Color WaypointLineColor { get; set; } = new Color("0900FF");
        public static Color CommandedWaypointLineColor { get; set; } = new Color("0900FF").Lightened(.2f);

        FleetSprite sprite;
        CollisionShape2D collisionShape;
        Line2D waypointsLine;

        #region Planet Stats

        public Cargo Cargo { get; } = new Cargo();
        public Planet Orbiting
        {
            get => orbiting; set
            {
                orbiting = value;
                if (collisionShape != null)
                {
                    // our collision shape is enabled if we are orbiting a planet
                    collisionShape.Disabled = orbiting != null;
                }
            }
        }
        Planet orbiting;

        public List<Waypoint> Waypoints { get; } = new List<Waypoint>();
        public List<ShipToken> Tokens { get; } = new List<ShipToken>();

        public FleetAggregate Aggregate { get; } = new FleetAggregate();

        #endregion

        public override void _Ready()
        {
            base._Ready();
            sprite = GetNode<FleetSprite>("Sprite");
            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape.Disabled = Orbiting != null;
            waypointsLine = GetNode<Line2D>("Waypoints");
        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }

        public override void UpdateSprite()
        {
            sprite?.UpdateSprite(Player, this);
            if (State == States.Active)
            {
                waypointsLine.DefaultColor = CommandedWaypointLineColor;
            }
            else
            {
                waypointsLine.DefaultColor = WaypointLineColor;
            }
        }

        public Waypoint AddWaypoint(MapObject mapObject)
        {
            var waypoint = new Waypoint(mapObject);

            Waypoints.Add(waypoint);

            // draw the line for this waypoint
            UpdateWaypointsLine();

            Signals.PublishFleetWaypointAddedEvent(this, waypoint);
            return waypoint;
        }

        void UpdateWaypointsLine()
        {
            Vector2[] points = Waypoints
                .Skip(1)
                .Prepend(new Waypoint() { Position = Position })
                .Select(wp => wp.Position - Position)
                .ToArray();
            waypointsLine.Points = points;
        }

        internal override List<MapObject> GetPeers()
        {
            List<MapObject> peers = new List<MapObject>();
            if (Orbiting != null)
            {
                // add any fleets after myself
                bool foundMe = false;
                Orbiting.OrbitingFleets.Where(f => f.OwnedByMe).ToList().ForEach(f =>
                {
                    if (f == this)
                    {
                        foundMe = true;
                        return;
                    }
                    if (foundMe)
                    {
                        peers.Add(f);
                    }
                });

                // add the planet as the final peer
                if (Orbiting.OwnedByMe)
                {
                    peers.Add(Orbiting);
                }
            }

            return peers;
        }

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

            // compute each token's 
            Tokens.ForEach(token =>
            {
                token.Design.ComputeAggregate(Player);
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
