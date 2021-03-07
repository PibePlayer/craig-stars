using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class WaypointArea : Area2D
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaypointArea));
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

        public override void _Ready()
        {
            // GlobalPosition = Target.GlobalPosition;
            // hook up mouse events to our area
            Connect("input_event", this, nameof(OnInputEvent));
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
        }

        void OnWaypointDeleted(Waypoint waypoint)
        {
            if (Waypoint == waypoint)
            {
                // I've been deleted, remove from queue
                GetParent()?.RemoveChild(this);
                DisconnectAll();
                QueueFree();
            }
        }

        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
        {
            if (IsInstanceValid(this))
            {
                if (@event.IsActionPressed("viewport_select"))
                {
                    log.Debug($"Selecting waypoint {Position}");
                    Signals.PublishWaypointSelectedEvent(Waypoint);
                }
            }
            else
            {
                log.Error("Got input event for invalid waypoint.");
            }
        }

        public void DisconnectAll()
        {
            Disconnect("input_event", this, nameof(OnInputEvent));
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
        }
    }
}