using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class OwnedFleetSummary : FleetSummary
    {
        Label shipCountLabel;
        Label massLabel;
        Label waypointLabel;
        Label waypointTaskLabel;
        Label warpLabel;
        Label mineSweepSummaryLabel;

        CargoBar fuelBar;
        CargoBar cargoBar;

        public override void _Ready()
        {
            base._Ready();

            shipCountLabel = (Label)FindNode("ShipCountLabel");
            massLabel = (Label)FindNode("MassLabel");
            waypointLabel = (Label)FindNode("WaypointLabel");
            waypointTaskLabel = (Label)FindNode("WaypointTaskLabel");
            warpLabel = (Label)FindNode("WarpLabel");
            mineSweepSummaryLabel = (Label)FindNode("MineSweepSummaryLabel");

            fuelBar = (CargoBar)FindNode("FuelBar");
            cargoBar = (CargoBar)FindNode("CargoBar");

            cargoBar.ValueUpdatedEvent += OnCargoBarPressed;

        }

        public override void _ExitTree()
        {
            cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
            base._ExitTree();
        }

        void OnCargoBarPressed(int newValue)
        {
            if (Fleet?.Fleet != null && Fleet.Fleet.Aggregate.CargoCapacity > 0 && Fleet?.Fleet?.Orbiting != null)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                Signals.PublishCargoTransferRequestedEvent(Fleet.Fleet, Fleet.Fleet.Orbiting);
            }
        }

        protected override void UpdateControls()
        {
            if (Fleet != null)
            {
                var race = Me.Race;
                var fleet = Fleet.Fleet;

                if (fleet.OwnedBy(Me))
                {
                    Visible = true;
                    shipCountLabel.Text = $"Ship Count: {fleet.Tokens.Count}";
                    massLabel.Text = $"Fleet Mass: {fleet.Aggregate.Mass}";
                    if (fleet.Waypoints.Count > 1)
                    {
                        var wp1 = fleet.Waypoints[1];
                        waypointLabel.Text = $"Next Waypoint: {wp1.TargetName}";
                        waypointTaskLabel.Text = $"Waypoint Task: {EnumUtils.GetLabelForWaypointTask(wp1.Task)}";
                        warpLabel.Text = $"Warp Speed: {wp1.WarpFactor}";
                    }
                    else
                    {
                        waypointLabel.Text = "Next Waypoint: (none)";
                        waypointTaskLabel.Text = "Waypoint Task: (no task here)";
                        warpLabel.Text = "Warp Speed: (stopped)";
                    }
                    // TODO: add minesweep summary
                    if (fleet.Aggregate.MineSweep > 0)
                    {
                        mineSweepSummaryLabel.Text = $"This fleet can destroy up to {fleet.Aggregate.MineSweep} mines per year.";
                    }
                    else
                    {
                        mineSweepSummaryLabel.Text = "";
                    }

                    fuelBar.Capacity = fleet.Aggregate.FuelCapacity;
                    fuelBar.Fuel = fleet.Fuel;
                    cargoBar.Cargo = fleet.Cargo;
                    cargoBar.Capacity = fleet.Aggregate.CargoCapacity;

                }
                else
                {
                    Visible = false;
                }
            }
        }
    }
}