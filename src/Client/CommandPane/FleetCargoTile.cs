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
            Signals.CargoTransferredEvent += OnCargoTransferred;

            base._Ready();

        }

        public override void _ExitTree()
        {
            cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
            Signals.CargoTransferredEvent -= OnCargoTransferred;
            base._ExitTree();
        }

        void OnCargoBarPressed(int newValue)
        {
            if (CommandedFleet?.Fleet != null && CommandedFleet.Fleet.Aggregate.CargoCapacity > 0 && CommandedFleet?.Fleet?.Orbiting != null)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                Signals.PublishCargoTransferRequestedEvent(CommandedFleet.Fleet, CommandedFleet.Fleet.Orbiting);
            }
        }

        void OnCargoTransferred(Fleet source, ICargoHolder dest)
        {
            UpdateControls();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                fuelBar.Capacity = CommandedFleet.Fleet.Aggregate.FuelCapacity;
                fuelBar.Fuel = CommandedFleet.Fleet.Fuel;
                cargoBar.Cargo = CommandedFleet.Fleet.Cargo;
                cargoBar.Capacity = CommandedFleet.Fleet.Aggregate.CargoCapacity;
                cargoGrid.Cargo = CommandedFleet.Fleet.Cargo;
            }
        }

    }
}