using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CraigStars
{

    public class FleetSprite : MapObjectSprite
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        /// <summary>
        /// Convenience method so the code looks like Fleet.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public Fleet Fleet
        {
            get => MapObject as Fleet;
            set
            {
                MapObject = value;
            }
        }

        /// <summary>
        /// A planet sprite this fleet is orbiting
        /// </summary>
        /// <value></value>
        public PlanetSprite Orbiting { get; set; }

        CollisionShape2D collisionShape;
        Line2D waypointsLine;

        Sprite selected;
        Sprite active;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            base._Ready();
            selected = GetNode<Sprite>("Sprite/Selected");
            active = GetNode<Sprite>("Sprite/Active");

            stateSprites.Add(selected);
            stateSprites.Add(active);

            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape.Disabled = Orbiting != null;
            waypointsLine = GetNode<Line2D>("Waypoints");
            UpdateWaypointsLine();

        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }

        public Waypoint AddWaypoint(MapObject mapObject)
        {
            var waypoint = new Waypoint(mapObject);

            Fleet.Waypoints.Add(waypoint);

            UpdateWaypointsLine();

            Signals.PublishFleetWaypointAddedEvent(Fleet, waypoint);
            return waypoint;
        }


        void UpdateWaypointsLine()
        {
            if (Fleet != null)
            {
                Vector2[] points = Fleet.Waypoints
                    .Skip(1)
                    .Prepend(new Waypoint() { Position = Position })
                    .Select(wp => wp.Position - Position)
                    .ToArray();
                waypointsLine.Points = points;
            }
        }

        public override List<MapObjectSprite> GetPeers()
        {
            List<MapObjectSprite> peers = new List<MapObjectSprite>();
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

        public override void UpdateSprite()
        {
            // turn them all off

            stateSprites.ForEach(s => s.Visible = false);

            // if we are orbiting a planet, don't show any sprites
            if (Orbiting != null)
            {
                return;
            }

            Sprite shipSprite = State == ScannerState.Active ? active : selected;
            shipSprite.Visible = true;

            var ownerAllyState = ScannerOwnerAlly.Unknown;
            if (OwnedByMe)
            {
                ownerAllyState = ScannerOwnerAlly.Owned;
            }
            else
            {
                ownerAllyState = ScannerOwnerAlly.Enemy;
            }

            switch (ownerAllyState)
            {
                case ScannerOwnerAlly.Owned:
                    shipSprite.Modulate = Colors.Blue;
                    break;
                case ScannerOwnerAlly.Friend:
                    shipSprite.Modulate = Colors.Yellow;
                    break;
                case ScannerOwnerAlly.Enemy:
                    shipSprite.Modulate = Colors.Red;
                    break;
                default:
                    shipSprite.Modulate = Colors.Gray;
                    break;
            }

            // update the waypoints line 
            if (State == ScannerState.Active)
            {
                waypointsLine.DefaultColor = GUIColors.CommandedWaypointLineColor;
            }
            else
            {
                waypointsLine.DefaultColor = GUIColors.WaypointLineColor;
            }
        }
    }

}