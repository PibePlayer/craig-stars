using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Utils;

namespace CraigStars.Client
{

    public class FleetSprite : MapObjectSprite
    {
        [Inject] protected FleetService fleetService;

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
                if (value != null && countLabel != null)
                {
                    countLabel.Text = $"{Fleet.Tokens.Sum(token => token.Quantity)}";
                }
            }
        }

        public override bool Commandable { get => true; }

        /// <summary>
        /// A planet sprite this fleet is orbiting
        /// </summary>
        /// <value></value>
        public PlanetSprite Orbiting
        {
            get => orbiting;
            set
            {
                orbiting = value;
                if (collisionShape != null)
                {
                    collisionShape.Disabled = Orbiting != null;
                }
            }
        }
        PlanetSprite orbiting;

        public List<FleetSprite> OtherFleets { get; set; } = new List<FleetSprite>();

        /// <summary>
        /// True if this fleet is currently filtered out and should not be displayed
        /// around planets 
        /// </summary>
        /// <value></value>
        public bool FilteredOut
        {
            get
            {
                return Me.UISettings.ShowIdleFleetsOnly && Fleet.Waypoints.Count > 1;
            }
        }

        CollisionShape2D collisionShape;
        Line2D waypointsLine;

        Node2D spriteContainer;
        Sprite selected;
        Sprite active;
        Label countLabel;

        List<Sprite> stateSprites;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            selected = GetNode<Sprite>("Sprite/Selected");
            active = GetNode<Sprite>("Sprite/Active");
            spriteContainer = GetNode<Node2D>("Sprite");
            countLabel = GetNode<Label>("CountLabel");

            stateSprites = new List<Sprite>() {
                selected,
                active
            };

            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            waypointsLine = GetNode<Line2D>("Waypoints");
            UpdateWaypointsLine();
            UpdateSprite();
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
                var color = PlayerColor;
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

        public void OnWaypointMoved()
        {
            UpdateWaypointsLine();
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
            int warpFactor = fleetService.GetDefaultWarpFactor(Fleet, Me);
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
            // TODO: make this better, my fleets keep running out of fuel.
            // waypoint.WarpFactor = fleetService.GetBestWarpFactor(Fleet, Me, Fleet.Waypoints[index], waypoint);

            UpdateWaypointsLine();

            EventManager.PublishWaypointAddedEvent(Fleet, waypoint);
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

            int warpFactor = fleetService.GetDefaultWarpFactor(Fleet, Me);
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
            // waypoint.WarpFactor = fleetService.GetBestWarpFactor(Fleet, Me, Fleet.Waypoints[index], waypoint);

            UpdateWaypointsLine();

            EventManager.PublishWaypointAddedEvent(Fleet, waypoint);
        }

        public void DeleteWaypoint(Waypoint waypoint)
        {
            var index = Fleet.Waypoints.FindIndex(wp => wp == waypoint);

            // don't delete the first index
            if (index > 0)
            {
                Fleet.Waypoints.RemoveAt(index);
                UpdateWaypointsLine();
                EventManager.PublishWaypointDeletedEvent(waypoint);

                // select the previous waypoint in the list
                EventManager.PublishWaypointSelectedEvent(Fleet.Waypoints[index - 1]);
            }
        }

        public void UpdateWaypointsLine()
        {
            if (Fleet != null && waypointsLine != null)
            {
                Vector2[] points = Fleet.Waypoints
                    .Skip(1)
                    .Prepend(new Waypoint() { Position = Position })
                    .Select(wp => wp.Position - Position)
                    .ToArray();
                waypointsLine.Points = points;
            }
        }

        public override void UpdateSprite()
        {
            if (!IsInstanceValid(this) || waypointsLine == null)
            {
                return;
            }

            // turn them all off
            stateSprites.ForEach(s => s.Visible = false);


            // update the waypoints line 
            if (State == ScannerState.Commanded)
            {
                waypointsLine.DefaultColor = GUIColorsProvider.Colors.CommandedWaypointLineColor;
                waypointsLine.ZAsRelative = false;
                waypointsLine.ZIndex = 10;
            }
            else
            {
                waypointsLine.DefaultColor = GUIColorsProvider.Colors.WaypointLineColor;
                waypointsLine.ZAsRelative = true;
                waypointsLine.ZIndex = 0;
            }

            var filteredOut = FilteredOut;
            collisionShape.Disabled = filteredOut;
            waypointsLine.Visible = !filteredOut;
            countLabel.Visible = Orbiting == null && Me.UISettings.ShowFleetTokenCounts && !filteredOut;
            countLabel.Text = $"{Fleet.Tokens.Sum(token => token.Quantity)}";

            // if we are orbiting a planet, don't show any sprites
            if (Orbiting != null || filteredOut)
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
                    shipSprite.Modulate = PlayerColor;
                    break;
                default:
                    shipSprite.Modulate = Colors.Gray;
                    break;
            }

            Update();
        }
    }


}