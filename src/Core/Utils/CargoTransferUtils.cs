using System;
using System.Collections.Generic;

namespace CraigStars.Utils
{
    public static class CargoTransferUtils
    {
        /// <summary>
        /// Create an CargoTransferOrder that will execute as wp-1 on the server when the turn is generated
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cargo"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CreateCargoTransferOrder(Player player, Cargo cargo, Fleet source, ICargoHolder dest)
        {
            // make an immediate CargoTransferOrder
            var order = new CargoTransferOrder()
            {
                Source = source,
                Dest = dest,
                Transfer = cargo,
            };

            player.CargoTransferOrders.Add(order);
            player.ImmediateFleetOrders.Add(order);
        }
    }
}