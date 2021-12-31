using System;
using Godot;
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

        [JsonIgnore]
        public int FuelCapacity { get => 0; }

        public Salvage()
        {
            Name = "Salvage";
        }

        /// <summary>
        /// Transfer to/from the cargo (no fuel is transferred)
        /// </summary>
        /// <param name="newCargo"></param>
        /// <param name="newFuel"></param>
        /// <returns></returns>
        public CargoTransferResult Transfer(Cargo newCargo, int newFuel = 0)
        {
            // we can't transfer to a mineral packet, only away from
            var transfered = new Cargo(
                Mathf.Clamp(newCargo.Ironium, -Cargo.Ironium, int.MaxValue),
                Mathf.Clamp(newCargo.Boranium, -Cargo.Boranium, int.MaxValue),
                Mathf.Clamp(newCargo.Germanium, -Cargo.Germanium, int.MaxValue),
                Mathf.Clamp(newCargo.Colonists, -Cargo.Colonists, int.MaxValue)
            );

            // transfer the cargo
            Cargo += transfered;

            return new CargoTransferResult(transfered, 0);
        }
    }
}
