using System;
using Godot;

namespace CraigStars.Client
{
    public delegate void CargoTransferRequested(Cargo newCargo, int newFuel);

    public interface ICargoTransferControl
    {
        event CargoTransferRequested CargoTransferRequestedEvent;

        Cargo Cargo { get; set; }
        int Fuel { get; set; }
        ICargoHolder CargoHolder { get; set; }

        void UpdateControls();

    }
}