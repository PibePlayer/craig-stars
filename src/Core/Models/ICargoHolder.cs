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
        /// Attempt to transfer cargo from cargoHolder to this object
        /// </summary>
        /// <param name="transfer">The cargo to transfer</param>
        /// <returns></returns>
        bool AttemptTransfer(Cargo transfer);
    }
}