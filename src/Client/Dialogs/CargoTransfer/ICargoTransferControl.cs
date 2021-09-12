using Godot;
using System;

namespace CraigStars
{
    public delegate void CargoTransferRequested(Cargo newCargo, int newFuel);

    public interface ICargoTransferControl
    {
        event CargoTransferRequested CargoTransferRequestedEvent;

        Cargo Cargo { get; set; }
        int Fuel { get; set; }
        ICargoHolder CargoHolder { get; set; }

        /// <summary>
        /// Attempt to transfer this cargo amount to or from the
        /// control
        /// </summary>
        /// <param name="newCargo"></param>
        /// <returns>True if this cargo was accepted</returns>
        bool AttemptTransfer(Cargo newCargo, int newFuel);
    }
}