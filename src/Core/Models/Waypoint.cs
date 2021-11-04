using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Waypoint
    {
        public const int StargateWarpFactor = 11;
        [JsonProperty(IsReference = true)]
        public MapObject Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null)
                {
                    Position = target.Position;
                }
            }
        }
        MapObject target;

        [JsonProperty(IsReference = true)]
        public MapObject OriginalTarget
        {
            get => originalTarget;
            set
            {
                originalTarget = value;
                if (originalTarget != null)
                {
                    Position = originalTarget.Position;
                }
            }
        }
        MapObject originalTarget;

        public Vector2 Position { get; set; }
        public Vector2 OriginalPosition { get; set; }

        public int WarpFactor { get; set; }

        public WaypointTask Task { get; set; }

        public WaypointTransportTasks TransportTasks { get; set; }

        public bool TaskComplete { get; set; }

        public Waypoint() { }

        public Waypoint(MapObject target, Vector2 position, int warpFactor = 5, WaypointTask task = WaypointTask.None, WaypointTransportTasks transportTasks = new WaypointTransportTasks())
        {
            this.target = target;
            if (target != null)
            {
                Position = target.Position;
            }
            else
            {
                // note: we must have a position or a target
                // TODO: throw an exception here?
                Position = position;
            }
            TransportTasks = transportTasks;
            WarpFactor = warpFactor;
            Task = task;
        }

        /// <summary>
        /// Create a clone of this waypoint. This is used when splitting fleets
        /// </summary>
        /// <returns></returns>
        public Waypoint Clone()
        {
            return new Waypoint()
            {
                Target = Target,
                Position = Position,
                WarpFactor = WarpFactor,
                Task = Task,
                TransportTasks = TransportTasks
            };
        }

        public static Waypoint TargetWaypoint(MapObject target, int warpFactor = 5, WaypointTask task = WaypointTask.None, WaypointTransportTasks? transportTasks = null)
        {
            return new Waypoint(target, Vector2.Zero, warpFactor, task, transportTasks == null ? new WaypointTransportTasks() : transportTasks.Value);
        }

        public static Waypoint PositionWaypoint(Vector2 position, int warpFactor = 5, WaypointTask task = WaypointTask.None, WaypointTransportTasks? transportTasks = null)
        {
            return new Waypoint(null, position, warpFactor, task, transportTasks == null ? new WaypointTransportTasks() : transportTasks.Value);
        }

        /// <summary>
        /// A string description of the target
        /// </summary>
        /// <value></value>
        public string TargetName
        {
            get
            {
                if (Target != null)
                {
                    return Target.Name;
                }
                else
                {
                    return $"Space: ({Position.x:.##}, {Position.y:.##})";
                }
            }
        }

        /// <summary>
        /// Return the distance from this waypoint to another waypoint, in light years.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public float GetDistanceToWaypoint(Waypoint to)
        {
            return Position.DistanceTo(to.Position);
        }

        /// <summary>
        /// Return the time, in years, to travel to this waypoint
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public float GetTimeToWaypoint(Waypoint to)
        {
            var distanceTraveled = to.WarpFactor * to.WarpFactor;
            return GetDistanceToWaypoint(to) / distanceTraveled;
        }

    }
}