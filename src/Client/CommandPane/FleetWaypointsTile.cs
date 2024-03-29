using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
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
        CheckBox repeatOrdersCheckBox;
        OtherWaypointTargetsPopupMenu otherWaypointTargetsPopupMenu;

        public override void _Ready()
        {
            waypoints = (ItemList)FindNode("Waypoints");
            comingFrom = (Label)FindNode("ComingFrom");
            comingFromLabel = (Label)FindNode("ComingFromLabel");
            nextWaypoint = (Label)FindNode("NextWaypoint");
            nextWaypointLabel = (Label)FindNode("NextWaypointLabel");
            distanceLabel = (Label)FindNode("DistanceLabel");
            distance = (Label)FindNode("Distance");
            warpFactorText = (Label)FindNode("WarpFactorText");
            warpFactor = (WarpFactor)FindNode("WarpFactor");
            travelTime = (Label)FindNode("TravelTime");
            selectedWaypointGrid = (Control)FindNode("SelectedWaypointGrid");
            estimatedFuelUsage = (Label)FindNode("EstimatedFuelUsage");
            travelTimeLabel = (Label)FindNode("TravelTimeLabel");
            estimatedFuelUsageLabel = (Label)FindNode("EstimatedFuelUsageLabel");
            repeatOrdersCheckBox = (CheckBox)FindNode("RepeatOrdersCheckBox");
            otherWaypointTargetsPopupMenu = GetNode<OtherWaypointTargetsPopupMenu>("CanvasLayer/OtherWaypointTargetsPopupMenu");

            selectedWaypointGrid.Visible = false;
            base._Ready();

            waypoints.Connect("item_selected", this, nameof(OnItemSelected));
            waypoints.Connect("gui_input", this, nameof(OnWaypointsGUIInput));
            repeatOrdersCheckBox.Connect("toggled", this, nameof(OnRepeatOrdersCheckBoxToggled));
            warpFactor.WarpSpeedChangedEvent += OnWarpSpeedChanged;
            EventManager.WaypointMovedEvent += OnWaypointMoved;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                warpFactor.WarpSpeedChangedEvent -= OnWarpSpeedChanged;
                EventManager.WaypointMovedEvent -= OnWaypointMoved;
            }
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
            if (CommandedFleet != null && index >= 0 && index < CommandedFleet.Fleet.Waypoints.Count)
            {
                // Select this waypoint and let listeners know (like ourselves and the viewport)
                EventManager.PublishWaypointSelectedEvent(CommandedFleet.Fleet.Waypoints[index]);
            }
        }

        void OnRepeatOrdersCheckBoxToggled(bool toggled)
        {
            if (CommandedFleet != null)
            {
                CommandedFleet.Fleet.RepeatOrders = toggled;
            }
        }

        protected override void OnNewCommandedFleet()
        {
            base.OnNewCommandedFleet();
            // when we have a new active fleet, set the active waypoint to the
            // first waypoint
            ActiveWaypoint = CommandedFleet?.Fleet.Waypoints[0];
        }

        string GetItemText(Waypoint waypoint)
        {
            var text = waypoint.Target != null ? waypoint.Target.Name : $"Space: ({waypoint.Position.x}, {waypoint.Position.y})";

            return text;
        }

        void UpdateItemText(int index, Waypoint waypoint)
        {
            var text = GetItemText(waypoint);
            if (Me.MapObjectsByLocation.TryGetValue(waypoint.Position, out var mapObjectsAtLocation) && mapObjectsAtLocation.Count > 1)
            {
                waypoints.SetItemTooltipEnabled(index, true);
                waypoints.SetItemTooltip(index, @"Other units at this location. \nRight click to target a differnent fleet, planet, or other target.");
                waypoints.SetItemText(index, text + "*");
            }
            else
            {
                waypoints.SetItemTooltipEnabled(index, false);
                waypoints.SetItemText(index, text);
            }
        }

        void OnWaypointMoved(Fleet fleet, Waypoint waypoint)
        {
            var selectedIndices = waypoints.GetSelectedItems();
            if (selectedIndices.Length > 0 && selectedIndices[0] > 0 && selectedIndices[0] < waypoints.GetItemCount())
            {
                UpdateItemText(selectedIndices[0], waypoint);
            }
        }

        void OnWaypointsGUIInput(InputEvent @event)
        {
            if (CommandedFleet != null && @event.IsActionPressed("viewport_alternate_select"))
            {
                GetTree().SetInputAsHandled();
                otherWaypointTargetsPopupMenu.Show(CommandedFleet, ActiveWaypoint, (target) =>
                {
                    ActiveWaypoint.Target = target;
                    UpdateControls();
                });
            }
            else if (@event.IsActionReleased("viewport_alternate_select"))
            {
                otherWaypointTargetsPopupMenu.Hide();
            }

        }

        void OnOtherWaypointTargetsPopupMenuIdPressed(int id)
        {

        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                int index = 0;
                int selectedIndex = 0;
                waypoints.Clear();
                foreach (var wp in CommandedFleet.Fleet.Waypoints)
                {
                    waypoints.AddItem(GetItemText(wp));
                    UpdateItemText(index, wp);

                    if (ActiveWaypoint == wp)
                    {
                        selectedIndex = index;
                        waypoints.Select(index);
                    }
                    index++;
                }

                repeatOrdersCheckBox.Pressed = CommandedFleet.Fleet.RepeatOrders;

                if (CommandedFleet.Fleet.Waypoints.Count > 1)
                {
                    selectedWaypointGrid.Visible = true;
                    distanceLabel.Visible = true;
                    distance.Visible = true;
                    warpFactorText.Visible = true;
                    warpFactor.Visible = true;

                    // setup the warpFactor control to support stargates
                    var hasStargate = CommandedFleet?.Fleet?.Orbiting?.HasStargate;
                    warpFactor.HasStargate = hasStargate.HasValue && hasStargate.Value;
                    warpFactor.MaxWarpFactor = warpFactor.HasStargate ? 11 : 10;

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
                        from = CommandedFleet.Fleet.Waypoints[selectedIndex];
                        to = CommandedFleet.Fleet.Waypoints[selectedIndex + 1];
                        nextWaypointLabel.Visible = true;
                        nextWaypoint.Visible = true;
                        comingFromLabel.Visible = false;
                        comingFrom.Visible = false;
                        nextWaypoint.Text = $"{to.TargetName}";
                        warpFactorText.Visible = true;
                        warpFactor.Visible = false;
                        warpFactorText.Text = $"{(to.WarpFactor == Waypoint.StargateWarpFactor ? "Stargate" : $"{to.WarpFactor}")}";
                    }
                    else
                    {
                        from = CommandedFleet.Fleet.Waypoints[selectedIndex - 1];
                        to = CommandedFleet.Fleet.Waypoints[selectedIndex];
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
                    if (to.WarpFactor == Waypoint.StargateWarpFactor)
                    {
                        travelTime.Text = "1 year";
                        estimatedFuelUsage.Text = "none";
                    }
                    else
                    {
                        var fuelCost = fleetService.GetFuelCost(CommandedFleet.Fleet, Me, to.WarpFactor, waypointDistance);
                        travelTime.Text = $"{Math.Ceiling(from.GetTimeToWaypoint(to))} years";
                        estimatedFuelUsage.Text = $"{fuelCost}mg";
                        if (fuelCost > CommandedFleet.Fleet.Fuel)
                        {
                            estimatedFuelUsage.Modulate = Colors.Red;
                        }
                        else
                        {
                            estimatedFuelUsage.Modulate = Colors.White;
                        }
                    }
                }
                else
                {
                    nextWaypointLabel.Visible = false;
                    nextWaypoint.Visible = false;
                    comingFromLabel.Visible = true;
                    comingFrom.Visible = true;
                    comingFrom.Text = $"{CommandedFleet.Fleet.Waypoints[0].TargetName}";

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