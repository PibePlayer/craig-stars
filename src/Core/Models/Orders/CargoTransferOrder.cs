using Newtonsoft.Json;

namespace CraigStars
{
    public class CargoTransferOrder : FleetOrder
    {
        [JsonProperty(IsReference = true)]
        public ICargoHolder Dest { get; set; }
        public Cargo Transfer { get; set; }

    }
}