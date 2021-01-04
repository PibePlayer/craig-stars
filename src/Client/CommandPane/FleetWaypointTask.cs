using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars
{
    public class FleetWaypointTask : FleetTile
    {
        OptionButton waypointTask;
        Waypoint ActiveWaypoint { get; set; }

        public override void _Ready()
        {
            base._Ready();
            waypointTask = FindNode("WaypointTask") as OptionButton;

            foreach (WaypointTask task in Enum.GetValues(typeof(WaypointTask)))
            {
                waypointTask.AddItem(GetLabelForWaypointTask(task));
            }
            Signals.WaypointSelectedEvent += OnWaypointSelected;
        }

        string GetLabelForWaypointTask(WaypointTask task)
        {
            switch (task)
            {
                case WaypointTask.RemoteMining:
                    return "Remote Mining";
                case WaypointTask.MergeWithFleet:
                    return "Merge With Fleet";
                case WaypointTask.ScrapFleet:
                    return "Scrap Fleet";
                case WaypointTask.LayMineField:
                    return "Lay Mine Field";
                case WaypointTask.TransferFleet:
                    return "Transfer";
                default:
                    return task.ToString();
            }

        }

        public override void _ExitTree()
        {
            base._Ready();
            Signals.WaypointSelectedEvent -= OnWaypointSelected;
        }

        void OnWaypointSelected(Waypoint waypoint)
        {
            ActiveWaypoint = waypoint;
            UpdateControls();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (waypointTask != null && ActiveWaypoint != null)
            {
                waypointTask.Selected = (int)ActiveWaypoint.Task;
            }
        }

    }
}