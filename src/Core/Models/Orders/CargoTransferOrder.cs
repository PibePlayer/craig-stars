using System;
using Newtonsoft.Json;

namespace CraigStars
{
    public class CargoTransferOrder : ImmediateFleetOrder
    {
        public Guid DestGuid { get; set; }
        public Cargo Transfer { get; set; }
        public int FuelTransfer { get; set; }
    }
}