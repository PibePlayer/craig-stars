using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class WaypointArea : Area2D
    {
        public Waypoint Waypoint
        {
            get => waypoint;
            set
            {
                waypoint = value;
                Position = waypoint.Position;
            }
        }
        Waypoint waypoint;

        /// <summary>
        /// The waypoint index in the fleet waypoints
        /// </summary>
        public int Index { get; set; }

        public override void _Ready()
        {
            // GlobalPosition = Target.GlobalPosition;
            // hook up mouse events to our area
            Connect("input_event", this, nameof(OnInputEvent));
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
        }

        public override void _ExitTree()
        {
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
        }

        void OnWaypointDeleted(Waypoint waypoint, int index)
        {
            if (Index == index)
            {
                // I've been deleted, remove from queue
                QueueFree();
            }
        }

        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                GD.Print($"Selecting waypoint {Position}");
                Signals.PublishWaypointSelectedEvent(Waypoint, Index);
            }
        }

    }
}