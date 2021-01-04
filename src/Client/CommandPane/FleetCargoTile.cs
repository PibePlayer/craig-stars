using Godot;
using System;

namespace CraigStars
{
    public class FleetCargoTile : FleetTile
    {
        CargoBar fuelBar;
        CargoBar cargoBar;
        CargoGrid cargoGrid;

        public override void _Ready()
        {
            fuelBar = FindNode("FuelBar") as CargoBar;
            cargoBar = FindNode("CargoBar") as CargoBar;
            cargoGrid = FindNode("CargoGrid") as CargoGrid;

            base._Ready();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActiveFleet != null)
            {
                fuelBar.Capacity = ActiveFleet.Fleet.Aggregate.FuelCapacity;
                fuelBar.Cargo = ActiveFleet.Fleet.Cargo;
                cargoBar.Cargo = ActiveFleet.Fleet.Cargo;
                cargoBar.Capacity = ActiveFleet.Fleet.Aggregate.CargoCapacity;
                cargoGrid.Cargo = ActiveFleet.Fleet.Cargo;
            }
        }

    }
}