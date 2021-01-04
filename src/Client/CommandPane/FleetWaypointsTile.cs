using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;

public class FleetWaypointsTile : FleetTile
{
    ItemList waypoints;
    Control selectedWaypointGrid;
    Label comingFrom;
    Label comingFromLabel;
    Label nextWaypoint;
    Label nextWaypointLabel;
    Label distance;
    Label warpFactor;
    Label travelTime;
    Label estimatedFuelUsage;


    Waypoint ActiveWaypoint { get; set; }

    public override void _Ready()
    {
        waypoints = FindNode("Waypoints") as ItemList;
        comingFrom = FindNode("ComingFrom") as Label;
        comingFromLabel = FindNode("ComingFromLabel") as Label;
        nextWaypoint = FindNode("NextWaypoint") as Label;
        nextWaypointLabel = FindNode("NextWaypointLabel") as Label;
        distance = FindNode("Distance") as Label;
        warpFactor = FindNode("WarpFactor") as Label;
        travelTime = FindNode("TravelTime") as Label;
        selectedWaypointGrid = FindNode("SelectedWaypointGrid") as Control;
        estimatedFuelUsage = FindNode("EstimatedFuelUsage") as Label;

        selectedWaypointGrid.Visible = false;
        base._Ready();

        waypoints.Connect("item_selected", this, nameof(OnItemSelected));

        Signals.FleetWaypointAddedEvent += OnFleetWaypointAdded;
        Signals.WaypointSelectedEvent += OnWaypointSelected;
        Signals.WaypointDeletedEvent += OnWaypointDeleted;
    }

    public override void _ExitTree()
    {
        Signals.FleetWaypointAddedEvent -= OnFleetWaypointAdded;
        Signals.WaypointSelectedEvent -= OnWaypointSelected;
        Signals.WaypointDeletedEvent -= OnWaypointDeleted;
    }

    void OnWaypointDeleted(Waypoint waypoint)
    {
        if (ActiveWaypoint == waypoint)
        {
            ActiveWaypoint = null;
        }
        UpdateControls();
    }

    void OnFleetWaypointAdded(Fleet fleet, Waypoint waypoint)
    {
        ActiveWaypoint = waypoint;
        UpdateControls();
    }

    void OnWaypointSelected(Waypoint waypoint)
    {
        ActiveWaypoint = waypoint;
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
                waypoints.AddItem(wp.Target != null ? wp.Target.ObjectName : $"Space: ({wp.Position.x}, {wp.Position.y})");
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
                    warpFactor.Text = $"{to.WarpFactor}";
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

                    // TODO: make a warp control
                    warpFactor.Text = $"{to.WarpFactor}";
                }

                var waypointDistance = Math.Abs(from.Position.DistanceTo(to.Position));
                distance.Text = $"{waypointDistance:.##} l.y.";
                travelTime.Text = $"{Math.Ceiling(from.GetTimeToWaypoint(to))} years";
                estimatedFuelUsage.Text = $"{ActiveFleet.Fleet.GetFuelCost(to.WarpFactor, waypointDistance)}mg";
            }

        }
    }

}
