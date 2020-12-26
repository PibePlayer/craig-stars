using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;

public class FleetWaypointsTile : FleetTile
{
    ItemList waypoints;
    Label comingFrom;

    public override void _Ready()
    {
        waypoints = FindNode("Waypoints") as ItemList;
        comingFrom = FindNode("ComingFrom") as Label;
        base._Ready();

        Signals.FleetWaypointAddedEvent += OnFleetWaypointAdded;

    }

    public override void _ExitTree()
    {
        Signals.FleetWaypointAddedEvent -= OnFleetWaypointAdded;
    }

    void OnFleetWaypointAdded(Fleet fleet, Waypoint waypoint)
    {
        UpdateControls();
    }

    protected override void UpdateControls()
    {
        base.UpdateControls();
        if (ActiveFleet != null)
        {
            waypoints.Clear();
            ActiveFleet.Waypoints.ForEach(wp => waypoints.AddItem(wp.Target != null ? wp.Target.ObjectName : $"Space: ({wp.TargetPos.x}, {wp.TargetPos.y})"));
            comingFrom.Text = $"{ActiveFleet.Waypoints[0].Target.ObjectName}";
        }
    }

}
