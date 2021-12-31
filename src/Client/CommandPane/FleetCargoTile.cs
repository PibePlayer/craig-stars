using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
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
            EventManager.CargoTransferredEvent += OnCargoTransferred;

            base._Ready();

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
                EventManager.CargoTransferredEvent -= OnCargoTransferred;
            }
        }

        void OnCargoBarPressed(int newValue)
        {
            if (CommandedFleet?.Fleet != null && CommandedFleet.Fleet.Spec.CargoCapacity > 0)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                EventManager.PublishCargoTransferDialogRequestedEvent(CommandedFleet.Fleet, CommandedFleet.Fleet.Orbiting);
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
                fuelBar.Capacity = CommandedFleet.Fleet.Spec.FuelCapacity;
                fuelBar.Fuel = CommandedFleet.Fleet.Fuel;
                cargoBar.Cargo = CommandedFleet.Fleet.Cargo;
                cargoBar.Capacity = CommandedFleet.Fleet.Spec.CargoCapacity;
                cargoGrid.Cargo = CommandedFleet.Fleet.Cargo;
            }
        }

    }
}