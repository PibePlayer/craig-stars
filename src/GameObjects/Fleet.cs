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
        FleetSprite sprite;
        CollisionShape2D collisionShape;
        Line2D waypointsLine;
        PackedScene waypointScene;

        #region Planet Stats

        public Player Player { get; set; }
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
        public List<ShipStack> ShipStacks { get; } = new List<ShipStack>();

        public FleetAggregate Aggregate { get; } = new FleetAggregate();

        #endregion

        public override void _Ready()
        {
            base._Ready();
            waypointScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Waypoint.tscn");
            sprite = GetNode<FleetSprite>("Sprite");
            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape.Disabled = Orbiting != null;

            // set our starting waypoint
            Waypoints.Clear();
            waypointsLine = GetNode<Line2D>("Waypoints");
            if (Orbiting != null)
            {
                Waypoints.Add(new Waypoint(Orbiting));
            }
            else
            {
                Waypoints.Add(new Waypoint()
                {
                    Position = Position
                });
            }
        }

        public Waypoint AddWaypoint(MapObject mapObject)
        {
            var waypoint = new Waypoint(mapObject);

            Waypoints.Add(waypoint);

            Vector2[] points = Waypoints
                .Skip(1)
                .Prepend(new Waypoint() { Position = Position })
                .Select(wp => wp.Position - Position)
                .ToArray();
            waypointsLine.Points = points;

            Signals.PublishFleetWaypointAddedEvent(this, waypoint);
            return waypoint;
        }

        internal override List<MapObject> GetPeers()
        {
            List<MapObject> peers = new List<MapObject>();
            if (Orbiting != null)
            {
                // add any fleets after myself
                bool foundMe = false;
                Orbiting.OrbitingFleets.Where(f => f.Player == Player).ToList().ForEach(f =>
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
                peers.Add(Orbiting);
            }

            return peers;
        }

        /// <summary>
        /// Fuel usage calculation courtesy of m.a@stars
        /// </summary>
        /// <param name = "speed" > The warp speed 1 to 10</param>
        /// <param name = "mass" > The mass of the fleet</param>
        /// <param name = "dist" > The distance travelled</param>
        /// <param name = "ifeFactor" > The factor for improved fuel efficiency (.85 if you have the LRT)</param>
        /// <param name = "engine" > The engine being used</param>
        /// <return> The amount of mg of fuel used</return>
        internal int GetFuelCost(int speed, int mass, double dist, double ifeFactor, TechEngine engine)
        {
            // 1 mg of fuel will move 200 kt of weight 1 LY at a Fuel Usage Number of 100.
            // Number of engines doesn't matter. Neither number of ships with the same engine.

            double distanceCeiling = Math.Ceiling(dist); // rounding to next integer gives best graph fit
                                                         // window.status = 'Actual distance used is ' + Distan + 'ly';

            // IFE is applied to drive specifications, just as the helpfile hints.
            // Stars! probably does it outside here once per turn per engine to save time.
            double engineEfficiency = Math.Ceiling(ifeFactor * engine.FuelUsage[speed - 1]);

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

        public int GetFuelCost(int speed, double dist)
        {
            // figure out how much fuel we're going to use
            double ifeFactor = Player.Race.HasLRT(LRT.IFE) ? .85 : 1.0;

            int fuelCost = 0;

            // compute each ship stack separately
            foreach (var stack in ShipStacks)
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
                fuelCost += GetFuelCost(speed, mass, dist, ifeFactor, stack.Design.Aggregate.Engine);
            }

            return fuelCost;
        }


        protected override void OnSelected()
        {
            sprite.UpdateVisibleSprites(Player, this);
        }

        protected override void OnDeselected()
        {
            sprite.UpdateVisibleSprites(Player, this);
        }

    }
}
