using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class FleetWaypointsTile : FleetWaypointTile
    {
        ItemList waypoints;
        Control selectedWaypointGrid;
        Label comingFrom;
        Label comingFromLabel;
        Label nextWaypoint;
        Label nextWaypointLabel;
        Label distanceLabel;
        Label distance;
        Label warpFactorText;
        WarpFactor warpFactor;
        Label travelTime;
        Label travelTimeLabel;
        Label estimatedFuelUsage;
        Label estimatedFuelUsageLabel;

        public override void _Ready()
        {
            waypoints = FindNode("Waypoints") as ItemList;
            comingFrom = FindNode("ComingFrom") as Label;
            comingFromLabel = FindNode("ComingFromLabel") as Label;
            nextWaypoint = FindNode("NextWaypoint") as Label;
            nextWaypointLabel = FindNode("NextWaypointLabel") as Label;
            distanceLabel = FindNode("DistanceLabel") as Label;
            distance = FindNode("Distance") as Label;
            warpFactorText = FindNode("WarpFactorText") as Label;
            warpFactor = FindNode("WarpFactor") as WarpFactor;
            travelTime = FindNode("TravelTime") as Label;
            selectedWaypointGrid = FindNode("SelectedWaypointGrid") as Control;
            estimatedFuelUsage = FindNode("EstimatedFuelUsage") as Label;
            travelTimeLabel = FindNode("TravelTimeLabel") as Label;
            estimatedFuelUsageLabel = FindNode("EstimatedFuelUsageLabel") as Label;

            selectedWaypointGrid.Visible = false;
            base._Ready();

            waypoints.Connect("item_selected", this, nameof(OnItemSelected));
            warpFactor.WarpSpeedChangedEvent += OnWarpSpeedChanged;
            Signals.WaypointMovedEvent += OnWaypointMoved;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            warpFactor.WarpSpeedChangedEvent -= OnWarpSpeedChanged;
            Signals.WaypointMovedEvent -= OnWaypointMoved;
        }

        void OnWarpSpeedChanged(int warpSpeed)
        {
            if (ActiveWaypoint != null)
            {
                ActiveWaypoint.WarpFactor = warpSpeed;
            }
            UpdateControls();
        }


        void OnItemSelected(int index)
        {
            if (ActiveFleet != null && index >= 0 && index < ActiveFleet.Fleet.Waypoints.Count)
            {
                // Select this waypoint and let listeners know (like ourselves and the viewport)
                Signals.PublishWaypointSelectedEvent(ActiveFleet.Fleet.Waypoints[index]);
            }
        }

        protected override void OnNewActiveFleet()
        {
            base.OnNewActiveFleet();
            // when we have a new active fleet, set the active waypoint to the
            // first waypoint
            ActiveWaypoint = ActiveFleet?.Fleet.Waypoints[0];
        }

        string GetItemText(Waypoint waypoint)
        {
            return waypoint.Target != null ? waypoint.Target.Name : $"Space: ({waypoint.Position.x}, {waypoint.Position.y})";
        }

        void OnWaypointMoved(Fleet fleet, Waypoint waypoint)
        {
            var selectedIndices = waypoints.GetSelectedItems();
            if (selectedIndices.Length > 0 && selectedIndices[0] > 0 && selectedIndices[0] < waypoints.GetItemCount())
            {
                waypoints.SetItemText(selectedIndices[0], GetItemText(waypoint));
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActiveFleet != null)
            {
                int index = 0;
                int selectedIndex = 0;
                waypoints.Clear();
                foreach (var wp in ActiveFleet.Fleet.Waypoints)
                {
                    waypoints.AddItem(GetItemText(wp));
                    if (ActiveWaypoint == wp)
                    {
                        selectedIndex = index;
                        waypoints.Select(index);
                    }
                    index++;
                }

                if (ActiveFleet.Fleet.Waypoints.Count > 1)
                {
                    selectedWaypointGrid.Visible = true;
                    distanceLabel.Visible = true;
                    distance.Visible = true;
                    warpFactorText.Visible = true;
                    warpFactor.Visible = true;
                    travelTime.Visible = true;
                    selectedWaypointGrid.Visible = true;
                    estimatedFuelUsage.Visible = true;
                    travelTimeLabel.Visible = true;
                    estimatedFuelUsageLabel.Visible = true;

                    // we show different controls for the first waypoint
                    // we show information about the next waypoint
                    Waypoint from;
                    Waypoint to;
                    if (selectedIndex == 0)
                    {
                        from = ActiveFleet.Fleet.Waypoints[selectedIndex];
                        to = ActiveFleet.Fleet.Waypoints[selectedIndex + 1];
                        nextWaypointLabel.Visible = true;
                        nextWaypoint.Visible = true;
                        comingFromLabel.Visible = false;
                        comingFrom.Visible = false;
                        nextWaypoint.Text = $"{to.TargetName}";
                        warpFactorText.Visible = true;
                        warpFactor.Visible = false;
                        warpFactorText.Text = $"{to.WarpFactor}";
                    }
                    else
                    {
                        from = ActiveFleet.Fleet.Waypoints[selectedIndex - 1];
                        to = ActiveFleet.Fleet.Waypoints[selectedIndex];
                        nextWaypointLabel.Visible = false;
                        nextWaypoint.Visible = false;
                        comingFromLabel.Visible = true;
                        comingFrom.Visible = true;
                        comingFrom.Text = $"{from.TargetName}";

                        warpFactorText.Visible = false;
                        warpFactor.Visible = true;
                        warpFactor.WarpSpeed = to.WarpFactor;
                    }

                    var waypointDistance = Math.Abs(from.Position.DistanceTo(to.Position));
                    distance.Text = $"{waypointDistance:.##} l.y.";
                    travelTime.Text = $"{Math.Ceiling(from.GetTimeToWaypoint(to))} years";
                    estimatedFuelUsage.Text = $"{ActiveFleet.Fleet.GetFuelCost(to.WarpFactor, waypointDistance)}mg";
                }
                else
                {
                    nextWaypointLabel.Visible = false;
                    nextWaypoint.Visible = false;
                    comingFromLabel.Visible = true;
                    comingFrom.Visible = true;
                    comingFrom.Text = $"{ActiveFleet.Fleet.Waypoints[0].TargetName}";

                    distanceLabel.Visible = false;
                    distance.Visible = false;
                    warpFactorText.Visible = false;
                    warpFactor.Visible = false;
                    travelTime.Visible = false;
                    selectedWaypointGrid.Visible = false;
                    estimatedFuelUsage.Visible = false;
                    travelTimeLabel.Visible = false;
                    estimatedFuelUsageLabel.Visible = false;
                }

            }
        }

    }
}