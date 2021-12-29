using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// This service executes client side fleet orders on the server
    /// </summary>
    public class CargoTransferer
    {
        static CSLog log = LogProvider.GetLogger(typeof(CargoTransferer));

        /// <summary>
        /// Transfer cargo from the source to the dest. Anything the dest can't take is put back in the source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="cargo"></param>
        /// <param name="fuel"></param>
        /// <returns>The actual cargo transferred</returns>
        public CargoTransferResult Transfer(ICargoHolder source, ICargoHolder dest, Cargo cargo, int fuel)
        {
            // first take away from the source
            var sourceResult = source.Transfer(-cargo, -fuel);

            // The sourceResult is how much we successfully took from the source, so invert that and give it to the dest
            var destResult = dest.Transfer(-sourceResult.cargo, -sourceResult.fuel);

            // give back any difference we couldn't give to the dest, to the source
            var diff = sourceResult + destResult;
            source.Transfer(-diff.cargo, -diff.fuel);

            return sourceResult - diff;
        }

    }
}

