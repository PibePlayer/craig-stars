using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class WaypointArea : Area2D
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaypointArea));

        public Fleet Fleet { get; set; }

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

        bool waypointMoving;


        public override void _Ready()
        {
            // hook up mouse events to our area
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

        public void DisconnectAll()
        {
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
        }
    }
}