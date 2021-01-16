using Godot;
using System;
using CraigStars.Singletons;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CraigStars
{
    public class Waypoint : SerializableMapObject
    {
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
                }
            }
        }
        MapObject target;

        public Vector2 Position { get; set; }

        public int WarpFactor { get; set; } = 5;

        public WaypointTask Task { get; set; } = WaypointTask.None;

        public WaypointTransportTasks TransportTasks { get; set; }

        #region Serializer Helpers

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
                    return $"Space: ({Position.x}, {Position.y})";
                }
            }
        }

        public Guid? TargetGuid
        {
            get
            {
                if (targetGuid == null)
                {
                    targetGuid = Target?.Guid;
                }
                return targetGuid;
            }
            set
            {
                targetGuid = value;
            }
        }
        Guid? targetGuid;

        /// <summary>
        /// Prepare this object for serialization
        /// </summary>
        public void PreSerialize()
        {
            targetGuid = null;
        }

        /// <summary>
        /// After serialization, wire up values we stored by guid
        /// </summary>
        /// <param name="mapObjectsByGuid"></param>
        public void PostSerialize(Dictionary<Guid, MapObject> mapObjectsByGuid)
        {
            if (targetGuid.HasValue)
            {
                mapObjectsByGuid.TryGetValue(targetGuid.Value, out var target);
                Target = target;
            }
        }

        #endregion

        public Waypoint()
        {
        }

        public Waypoint(MapObject target, int warpFactor = 0, WaypointTask task = WaypointTask.None, WaypointTransportTasks transportTasks = null)
        {
            Target = target;
            WarpFactor = warpFactor;
            Task = task;
            TransportTasks = transportTasks;
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