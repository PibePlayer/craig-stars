using CraigStars.Singletons;
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

            cargoBar.ValueUpdatedEvent += OnCargoBarPressed;

            base._Ready();
        }

        public override void _ExitTree()
        {
            cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
            base._ExitTree();
        }

        void OnCargoBarPressed(int newValue)
        {
            if (ActiveFleet?.Fleet != null && ActiveFleet.Fleet.Aggregate.CargoCapacity > 0 && ActiveFleet?.Fleet?.Orbiting != null)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                Signals.PublishCargoTransferRequestedEvent(ActiveFleet.Fleet, ActiveFleet.Fleet.Orbiting);
            }
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