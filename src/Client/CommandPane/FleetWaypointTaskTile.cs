using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class FleetWaypointTaskTile : FleetWaypointTile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetWaypointTaskTile));

        OptionButton waypointTask;

        public override void _Ready()
        {
            base._Ready();
            waypointTask = (OptionButton)FindNode("WaypointTask");

            foreach (WaypointTask task in Enum.GetValues(typeof(WaypointTask)))
            {
                waypointTask.AddItem(GetLabelForWaypointTask(task));
            }

            waypointTask.Connect("item_selected", this, nameof(OnWaypointTaskItemSelected));
        }

        // public override void _ExitTree()
        // {
        //     base._Ready();
        // }

        protected override void OnNewActiveFleet()
        {
            base.OnNewActiveFleet();
            // when we have a new active fleet, set the active waypoint to the
            // first waypoint
            ActiveWaypoint = ActiveFleet?.Fleet.Waypoints[0];
        }


        void OnWaypointTaskItemSelected(int index)
        {
            if (ActiveWaypoint != null && index >= 0 && index < Enum.GetValues(typeof(WaypointTask)).Length)
            {
                log.Debug($"Changing waypoint {ActiveWaypoint.TargetName} from {ActiveWaypoint.Task} to {(WaypointTask)index}");
                ActiveWaypoint.Task = (WaypointTask)index;
            }
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


        protected override void UpdateControls()
        {
            base.UpdateControls();
            var wp = ActiveWaypoint;
            if (waypointTask != null && wp != null)
            {
                waypointTask.Selected = (int)wp.Task;
            }
        }

    }
}