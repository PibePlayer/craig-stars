using System;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A mineral packet flying through space
    /// </summary>
    public class MineralPacket : MapObject, ICargoHolder
    {
        [JsonProperty(IsReference = true)]
        public MapObject Target { get; set; }
        public Cargo Cargo { get; set; }
        public int SafeWarpSpeed { get; set; }
        public int WarpFactor { get; set; }
        public float DistanceTravelled { get; set; }
        public Vector2 Heading { get; set; }

        [JsonIgnore]
        public int AvailableCapacity { get => int.MaxValue; }

        [JsonIgnore]
        public int Fuel { get => 0; set { } }

        [JsonIgnore]
        public int FuelCapacity { get => 0; }

        /// <summary>
        /// Check if this cargo transfer is valid
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="fuelTransfer"></param>
        /// <returns></returns>
        public bool CheckTransfer(Cargo transfer, int fuel = 0)
        {
            if (fuel > 0 || fuel < 0)
            {
                // fleets can't transfer fuel to salvage
                return false;
            }
            if (transfer.Ironium > 0 || transfer.Boranium > 0 || transfer.Germanium > 0 || transfer.Colonists > 0)
            {
                // we can't be given cargo, it can only be sucked away
                return false;
            }

            return (Cargo + transfer) >= 0;
        }

        /// <summary>
        /// Transfer from the cargo (cannot put cargo in salvage or mineral packets)
        /// </summary>
        /// <param name="newCargo"></param>
        /// <param name="newFuel"></param>
        /// <returns></returns>
        public CargoTransferResult Transfer(Cargo newCargo, int newFuel = 0)
        {
            // we can't transfer to a mineral packet, only away from
            var transfered = new Cargo(
                Mathf.Clamp(newCargo.Ironium, -Cargo.Ironium, 0),
                Mathf.Clamp(newCargo.Boranium, -Cargo.Boranium, 0),
                Mathf.Clamp(newCargo.Germanium, -Cargo.Germanium, 0),
                Mathf.Clamp(newCargo.Colonists, -Cargo.Colonists, 0)
            );

            // transfer the cargo
            Cargo += transfered;

            return new CargoTransferResult(transfered, 0);
        }
    }
}
