using Godot;
using System;

namespace CraigStars
{
    public class Waypoint : Area2D
    {
        public MapObject Target { get; set; }
        public Vector2 TargetPos
        {
            get => (Target == null ? targetPos : Target.Position); set
            {
                targetPos = value;
            }
        }
        Vector2 targetPos;

        public int WarpSpeed { get; set; } = 5;

        public Vector2 GlobalPosition { get => Target != null ? Target.GlobalPosition : targetPos; }

        public Waypoint(MapObject target)
        {
            Target = target;
            TargetPos = target.Position;
        }

        public Waypoint(Vector2 target)
        {
            TargetPos = target;
        }
    }
}