using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Waypoint
    {
        public const int StargateWarpFactor = 11;
        public const int Indefinite = -1;
        public const int PatrolWarpFactorAutomatic = -1;
        public const int PatrolRangeInfinite = -1;

        /// <summary>
        /// Waypoints can target positions or other mapobjects. If a fleet is targetting a mapobject, it would go here
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public MapObject Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null)
                {
                    Position = target.Position;
                    TargetGuid = target.Guid;
                }
                else
                {
                    TargetGuid = null;
                }
            }
        }
        MapObject target;

        /// <summary>
        /// During serialization, this is used to lookup targets from the player or game MapObjects
        /// </summary>
        /// <value></value>
        public Guid? TargetGuid { get; set; }

        [JsonIgnore]
        public MapObject OriginalTarget
        {
            get => originalTarget;
            set
            {
                originalTarget = value;
                if (originalTarget != null)
                {
                    Position = originalTarget.Position;
                    OriginalTargetGuid = originalTarget.Guid;
                }
                else
                {
                    OriginalTargetGuid = null;
                }
            }
        }
        MapObject originalTarget;

        /// <summary>
        /// During serialization, this is used to lookup targets from the player or game MapObjects
        /// </summary>
        /// <value></value>
        public Guid? OriginalTargetGuid { get; set; }


        public Vector2 Position { get; set; }
        public Vector2 OriginalPosition { get; set; }

        public int WarpFactor { get; set; }

        public WaypointTask Task { get; set; }

        public WaypointTransportTasks TransportTasks { get; set; }

        /// <summary>
        /// The duration, in years, to lay mines before moving on to the next waypoint
        /// </summary>
        /// <value></value>
        [DefaultValue(Indefinite)]
        public int LayMineFieldDuration { get; set; } = Indefinite;

        /// <summary>
        /// The range, in light years, to intercept ships
        /// </summary>
        /// <value></value>
        [DefaultValue(50)]
        public int PatrolRange { get; set; } = 50;

        /// <summary>
        /// The warp factor to use when intercepting ships
        /// </summary>
        /// <value></value>
        [DefaultValue(PatrolWarpFactorAutomatic)]
        public int PatrolWarpFactor { get; set; } = PatrolWarpFactorAutomatic;

        /// <summary>
        /// The player number to transfer this fleet to
        /// </summary>
        /// <value></value>
        public int TransferToPlayer { get; set; } = -1;

        /// <summary>
        /// Set to true if the currently processed waypoint task should cause the fleet to wait at
        /// this waypoint
        /// </summary>
        /// <value></value>
        public bool WaitAtWaypoint { get; set; }

        public Waypoint() { }

        public Waypoint(
            MapObject target,
            Vector2 position,
            int warpFactor = 5,
            WaypointTask task = WaypointTask.None,
            WaypointTransportTasks transportTasks = new WaypointTransportTasks(),
            int layMineFieldDuration = Indefinite,
            int patrolRange = 50,
            int patrolWarpFactor = PatrolWarpFactorAutomatic,
            int transferToPlayer = -1
            )
        {
            this.target = target;
            if (target != null)
            {
                Position = target.Position;
                TargetGuid = target.Guid;
            }
            else
            {
                // note: we must have a position or a target
                // TODO: throw an exception here?
                Position = position;
                TargetGuid = null;
            }
            TransportTasks = transportTasks;
            LayMineFieldDuration = layMineFieldDuration;
            PatrolRange = patrolRange;
            PatrolWarpFactor = patrolWarpFactor;
            TransferToPlayer = transferToPlayer;
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
                TransportTasks = TransportTasks,
                LayMineFieldDuration = LayMineFieldDuration,
                PatrolRange = PatrolRange,
                PatrolWarpFactor = PatrolWarpFactor,
                TransferToPlayer = TransferToPlayer,
            };
        }

        public static Waypoint TargetWaypoint(
            MapObject target,
            int warpFactor = 5,
            WaypointTask task = WaypointTask.None,
            WaypointTransportTasks? transportTasks = null,
            int layMineFieldDuration = Indefinite,
            int patrolRange = 50,
            int patrolWarpFactor = PatrolWarpFactorAutomatic,
            int transferToPlayer = -1
            )
        {
            return new Waypoint(target, Vector2.Zero, warpFactor, task, transportTasks == null ? new WaypointTransportTasks() : transportTasks.Value, layMineFieldDuration, patrolRange, patrolWarpFactor, transferToPlayer);
        }

        public static Waypoint PositionWaypoint(
            Vector2 position,
            int warpFactor = 5,
            WaypointTask task = WaypointTask.None,
            WaypointTransportTasks? transportTasks = null,
            int layMineFieldDuration = Indefinite,
            int patrolRange = 50,
            int patrolWarpFactor = PatrolWarpFactorAutomatic,
            int transferToPlayer = -1
        )
        {
            return new Waypoint(null, position, warpFactor, task, transportTasks == null ? new WaypointTransportTasks() : transportTasks.Value, layMineFieldDuration, patrolRange, patrolWarpFactor, transferToPlayer);
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