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

        #region Planet Stats

        public Player Player { get; set; }
        public Cargo Cargo { get; set; }
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

        #endregion

        public override void _Ready()
        {
            base._Ready();
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
                Waypoints.Add(new Waypoint(Position));
            }
        }

        public void AddWaypoint(MapObject mapObject)
        {
            var waypoint = new Waypoint(mapObject);
            Waypoints.Add(waypoint);
            Vector2[] points = Waypoints.Skip(1).Prepend(new Waypoint(GlobalPosition)).Select(wp => wp.GlobalPosition - GlobalPosition).ToArray();
            waypointsLine.Points = points;

            Signals.PublishFleetWaypointAddedEvent(this, waypoint);
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
