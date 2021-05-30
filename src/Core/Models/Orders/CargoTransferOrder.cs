using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class CargoTransferOrder : FleetOrder
    {
        [JsonProperty(IsReference = true)]
        public ICargoHolder Dest { get; set; }
        public Cargo Transfer { get; set; }
        public int FuelTransfer { get; set; }

    }
}