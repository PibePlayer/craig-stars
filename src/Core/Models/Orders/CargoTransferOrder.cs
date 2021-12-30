using System;
using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class CargoTransferOrder : ImmediateFleetOrder
    {
        public Guid DestGuid { get; set; }
        public Cargo Transfer { get; set; }
        public int FuelTransfer { get; set; }
    }
}