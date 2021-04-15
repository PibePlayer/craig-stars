using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Salvage from battle or a scrapped ship
    /// </summary>
    [JsonObject(IsReference = true)]
    public class Salvage : MapObject, ICargoHolder
    {
        public Cargo Cargo { get; set; }

        [JsonIgnore]
        public int AvailableCapacity { get => int.MaxValue; }

        [JsonIgnore]
        public int Fuel { get => 0; set { } }

        public bool AttemptTransfer(Cargo transfer, int fuel)
        {
            if (fuel > 0 || fuel < 0)
            {
                // fleets can't transfer fuel to salvage
                return false;
            }

            var result = Cargo + transfer;
            if (result >= 0)
            {
                // The transfer doesn't leave us with 0 minerals, so allow it
                Cargo = result;
                return true;
            }
            return false;
        }
    }
}
