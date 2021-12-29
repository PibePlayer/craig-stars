using System;

namespace CraigStars
{
    public interface ICargoHolder
    {
        /// <summary>
        /// The Guid of this cargo holder, whether planet, fleet, scrap, etc
        /// </summary>
        /// <value></value>
        Guid Guid { get; set; }

        /// <summary>
        /// The name of this, for logging and messages
        /// </summary>
        /// <value></value>
        string Name { get; set; }

        /// <summary>
        /// All cargo holders must have cargo
        /// </summary>
        /// <value></value>
        Cargo Cargo { get; set; }

        /// <summary>
        /// The amount of cargo holder can hold
        /// </summary>
        int AvailableCapacity { get; }

        /// <summary>
        /// All cargo holders must have fuel
        /// </summary>
        /// <value></value>
        int Fuel { get; set; }

        /// <summary>
        /// All cargo holders must have fuel
        /// </summary>
        /// <value></value>
        int FuelCapacity { get; }

        /// <summary>
        /// Transfer cargo and fuel to/from this cargo holder. If the newCargo/Fuel values are negative, this will
        /// remove cargo. If positive it will add cargo
        /// </summary>
        /// <param name="newCargo">The cargo to transfer</param>
        /// <param name="newFuel">The fuel to transfer</param>
        /// <returns>The actual amount transferred</returns>
        CargoTransferResult Transfer(Cargo newCargo, int newFuel = 0);
    }
}