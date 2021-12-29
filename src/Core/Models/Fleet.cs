using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    interface IAggregatable<T> where T : ShipDesignSpec
    {
        T Spec { get; }
    }

    [JsonObject(IsReference = true)]
    public class Fleet : MapObject, SerializableMapObject, ICargoHolder, IAggregatable<FleetSpec>
    {
        static CSLog log = LogProvider.GetLogger(typeof(Fleet));

        /// <summary>
        /// Fleets are named "Long Range Scout #9"
        /// This is the base name without the id number
        /// </summary>
        /// <value></value>
        public string BaseName
        {
            get => baseName;
            set
            {
                baseName = value;
                Name = $"{baseName} #{Id}";
            }
        }
        string baseName;

        #region Stats

        public int Age { get; set; }
        public Cargo Cargo { get; set; } = new Cargo();
        [JsonIgnore]
        public int AvailableCapacity { get => Spec.CargoCapacity - Cargo.Total; }

        public int Fuel { get; set; }
        [JsonIgnore] public int FuelCapacity { get => Spec.FuelCapacity; }
        [JsonIgnore] public int FuelMissing { get => Spec.FuelCapacity - Fuel; }
        public int Damage { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet Orbiting { get; set; }
        [JsonIgnore] public List<Fleet> OtherFleets { get; set; } = new List<Fleet>();

        public bool Scrapped { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
        public bool RepeatOrders { get; set; }

        [JsonProperty(IsReference = true)]
        public BattlePlan BattlePlan { get; set; }

        [JsonProperty(IsReference = true)]
        public FleetComposition FleetComposition { get; set; } = new FleetComposition();

        // These are all publicly viewable when a fleet is scanned
        public List<ShipToken> Tokens { get; set; } = new List<ShipToken>();
        public Vector2 Heading { get; set; }
        public int WarpSpeed { get; set; }
        public int IdleTurns { get; set; }

        /// <summary>
        /// During movement, we keep track of the starting position of each fleet so we can 
        /// pen scan planets as we travel through
        /// </summary>
        /// <value></value>
        [JsonIgnore] public Vector2? PreviousPosition { get; set; }

        [JsonIgnore]
        public int Mass { get => Spec.Mass; set => Spec.Mass = value; }

        public FleetSpec Spec { get; set; } = new FleetSpec();

        #endregion

        /// <summary>
        /// Get the orders for this fleet
        /// </summary>
        /// <returns></returns>
        public FleetWaypointsOrders GetOrders()
        {
            return new FleetWaypointsOrders()
            {
                Guid = Guid,
                Tags = new(Tags),
                Waypoints = new(Waypoints),
                RepeatOrders = RepeatOrders,
                BaseName = BaseName,
                BattlePlanGuid = BattlePlan.Guid
            };
        }

        /// <summary>
        /// Get the primary shipToken, i.e. the one that is most powerful
        /// </summary>
        /// <returns></returns>
        public ShipToken GetPrimaryToken()
        {
            // TODO: write this code
            return Tokens.FirstOrDefault();
        }

        /// <summary>
        /// Transfer newCargo and newFuel into the fleet (or out of, if newCargo or newFuel is negative)
        /// </summary>
        /// <param name="newCargo">The cargo to transfer in or out of the fleet</param>
        /// <param name="newFuel"></param>
        /// <returns></returns>
        public CargoTransferResult Transfer(Cargo newCargo, int newFuel = 0)
        {
            // transfer this new cargo into the fleet, capping at capacity each time
            Cargo transferResult = new Cargo();
            // first transfer in/out new Ironium, but don't allow us to remove more ironium than we have, or gain more ironium than we have space for
            transferResult = transferResult.WithIronium(Mathf.Clamp(newCargo.Ironium, -Cargo.Ironium, Spec.CargoCapacity - (Cargo + transferResult).Total));
            // do the same for the other cargo, but use our transferResult in the total cargo check so we don't overload our capacity
            transferResult = transferResult.WithBoranium(Mathf.Clamp(newCargo.Boranium, -Cargo.Boranium, Spec.CargoCapacity - (Cargo + transferResult).Total));
            transferResult = transferResult.WithGermanium(Mathf.Clamp(newCargo.Germanium, -Cargo.Germanium, Spec.CargoCapacity - (Cargo + transferResult).Total));
            transferResult = transferResult.WithColonists(Mathf.Clamp(newCargo.Colonists, -Cargo.Colonists, Spec.CargoCapacity - (Cargo + transferResult).Total));

            // do the same for fuel
            var fuelResult = Mathf.Clamp(newFuel, -Fuel, FuelCapacity - Fuel);

            // update the cargo
            Cargo = Cargo + transferResult;
            Fuel = Fuel + fuelResult;
            return new CargoTransferResult(transferResult, fuelResult);
        }
    }
}
