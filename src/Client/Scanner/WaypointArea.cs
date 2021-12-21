using System;
using Godot;

namespace CraigStars.Client
{
    public class WaypointArea : Area2D
    {
        static CSLog log = LogProvider.GetLogger(typeof(WaypointArea));

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
            EventManager.WaypointDeletedEvent += OnWaypointDeleted;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Waypoint = null;
            Fleet = null;
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
            EventManager.WaypointDeletedEvent -= OnWaypointDeleted;
        }
    }
}