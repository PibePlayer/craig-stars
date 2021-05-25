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

        public override bool Commandable { get => true; }

        /// <summary>
        /// A planet sprite this fleet is orbiting
        /// </summary>
        /// <value></value>
        public PlanetSprite Orbiting { get; set; }

        public List<FleetSprite> OtherFleets { get; set; } = new List<FleetSprite>();

        CollisionShape2D collisionShape;
        Line2D waypointsLine;

        Node2D spriteContainer;
        Sprite selected;
        Sprite active;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            base._Ready();
            selected = GetNode<Sprite>("Sprite/Selected");
            active = GetNode<Sprite>("Sprite/Active");
            spriteContainer = GetNode<Node2D>("Sprite");

            stateSprites.Add(selected);
            stateSprites.Add(active);

            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape.Disabled = Orbiting != null;
            waypointsLine = GetNode<Line2D>("Waypoints");
            UpdateWaypointsLine();

            Signals.WaypointMovedEvent += OnWaypointMoved;

        }

        public override void _ExitTree()
        {
            base._ExitTree();

            Signals.WaypointMovedEvent -= OnWaypointMoved;
        }

        public override void _Draw()
        {
            if (!IsInstanceValid(this))
            {
                return;
            }
            // for fleets owned by other players, draw a helpful line showing how fast the fleet is going
            // and where it will end up
            if (State == ScannerState.Selected && !Fleet.OwnedBy(Me) && Fleet.WarpSpeed > 0)
            {
                var distancePerYear = Fleet.WarpSpeed * Fleet.WarpSpeed;
                var color = Fleet.Owner.Color;
                var perpendicular = Fleet.Heading.Perpendicular();
                for (var i = 0; i < 5; i++)
                {
                    DrawLine(Fleet.Heading * i * distancePerYear, Fleet.Heading * ((i + 1) * distancePerYear), color, 2);
                    DrawLine(-Fleet.Heading * i * distancePerYear, -Fleet.Heading * ((i + 1) * distancePerYear), color, 2);

                    // draw wings per each warp
                    DrawLine(Fleet.Heading * i * distancePerYear - perpendicular * 5, Fleet.Heading * i * distancePerYear + perpendicular * 5, color, 2);
                    DrawLine(-Fleet.Heading * i * distancePerYear - perpendicular * 5, -Fleet.Heading * i * distancePerYear + perpendicular * 5, color, 2);
                }
            }
        }

        /// <summary>
        /// Add a waypoint (after another waypoint)
        /// </summary>
        /// <param name="mapObject"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public void AddWaypoint(MapObject mapObject, Waypoint after = null)
        {
            var index = Fleet.Waypoints.Count - 1;
            if (after != null)
            {
                index = Fleet.Waypoints.FindIndex(wp => wp == after);
            }
            int warpFactor = Fleet.GetDefaultWarpFactor();
            WaypointTask task = Fleet.Waypoints[index].Task;
            WaypointTransportTasks transportTasks = Fleet.Waypoints[index].TransportTasks;
            if (index >= 0)
            {
                warpFactor = Fleet.Waypoints[index].WarpFactor;
            }
            var waypoint = Waypoint.TargetWaypoint(mapObject, warpFactor, task, transportTasks);

            if (Fleet.Waypoints[index].Target == mapObject || Fleet.Waypoints[index].Position == mapObject.Position)
            {
                return;
            }

            Fleet.Waypoints.Insert(index + 1, waypoint);

            UpdateWaypointsLine();

            Signals.PublishWaypointAddedEvent(Fleet, waypoint);
        }

        /// <summary>
        /// Add a waypoint at a position, after some current selected waypoint
        /// </summary>
        /// <param name="position"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public void AddWaypoint(Vector2 position, Waypoint after = null)
        {
            var index = Fleet.Waypoints.Count - 1;
            if (after != null)
            {
                index = Fleet.Waypoints.FindIndex(wp => wp == after);
            }

            int warpFactor = Fleet.GetDefaultWarpFactor();
            if (index >= 0)
            {
                warpFactor = Fleet.Waypoints[index].WarpFactor;
            }
            var waypoint = new Waypoint()
            {
                WarpFactor = warpFactor,
                Position = position
            };


            Fleet.Waypoints.Insert(index + 1, waypoint);

            UpdateWaypointsLine();

            Signals.PublishWaypointAddedEvent(Fleet, waypoint);
        }

        public void DeleteWaypoint(Waypoint waypoint)
        {
            var index = Fleet.Waypoints.FindIndex(wp => wp == waypoint);

            // don't delete the first index
            if (index > 0)
            {
                Fleet.Waypoints.RemoveAt(index);
                UpdateWaypointsLine();
                Signals.PublishWaypointDeletedEvent(waypoint);

                // select the previous waypoint in the list
                Signals.PublishWaypointSelectedEvent(Fleet.Waypoints[index - 1]);
            }
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

        void OnWaypointMoved(Fleet fleet, Waypoint waypoint)
        {
            if (fleet == Fleet)
            {
                UpdateWaypointsLine();
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
            if (!IsInstanceValid(this))
            {
                return;
            }

            // turn them all off
            stateSprites.ForEach(s => s.Visible = false);

            // update the waypoints line 
            if (State == ScannerState.Commanded)
            {
                waypointsLine.DefaultColor = GUIColors.CommandedWaypointLineColor;
                waypointsLine.ZAsRelative = false;
                waypointsLine.ZIndex = 10;
            }
            else
            {
                waypointsLine.DefaultColor = GUIColors.WaypointLineColor;
                waypointsLine.ZAsRelative = true;
                waypointsLine.ZIndex = 0;
            }

            // if we are orbiting a planet, don't show any sprites
            if (Orbiting != null)
            {
                Update();
                return;
            }

            Sprite shipSprite = State == ScannerState.Commanded ? active : selected;
            shipSprite.Visible = true;

            if (Fleet.WarpSpeed > 0)
            {
                // look at our heading
                spriteContainer.LookAt(GlobalPosition + Fleet.Heading);
            }
            else
            {
                spriteContainer.Rotation = 0;
            }

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
                case ScannerOwnerAlly.Enemy:
                    shipSprite.Modulate = Fleet.Owner.Color;
                    break;
                default:
                    shipSprite.Modulate = Colors.Gray;
                    break;
            }

            Update();
        }
    }


}