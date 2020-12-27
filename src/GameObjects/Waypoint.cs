using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class Waypoint
    {
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
                    return Target.ObjectName;
                }
                else
                {
                    return $"Space: ({Position.x}, {Position.y})";
                }
            }
        }

        public Waypoint()
        {
        }

        public Waypoint(MapObject target)
        {
            Target = target;
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